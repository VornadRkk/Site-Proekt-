using DateAndTime.Api.Contracts.Auth;
using DateAndTime.Api.Data;
using DateAndTime.Api.Services;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DateAndTime.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(AppDbContext db, PasswordHasher hasher, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("register"), EnableRateLimiting("admin-login")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request, CancellationToken cancellationToken) {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken)) return Conflict(new { message = "Registration unavailable for this email." });
        var user = new User { Id = Guid.NewGuid(), Email = email, DisplayName = request.DisplayName.Trim(), PasswordHash = hasher.Hash(request.Password), Role = UserRole.Client, CreatedAtUtc = DateTime.UtcNow };
        db.Users.Add(user);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Conflict(new { message = "Registration unavailable for this email." }); }
        var (token, expires) = tokens.Create(user);
        return Ok(new LoginResponse(token, expires, user.DisplayName, user.Role.ToString()));
    }

    [HttpGet("session"), Authorize]
    public async Task<IActionResult> Session(CancellationToken cancellationToken) {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
        return user is null ? Unauthorized() : Ok(new { user.DisplayName, Role = user.Role.ToString() });
    }

    [HttpPost("login"), EnableRateLimiting("admin-login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant() && x.IsActive, cancellationToken);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash)) return Unauthorized();
        var (token, expires) = tokens.Create(user);
        return Ok(new LoginResponse(token, expires, user.DisplayName, user.Role.ToString()));
    }
}
