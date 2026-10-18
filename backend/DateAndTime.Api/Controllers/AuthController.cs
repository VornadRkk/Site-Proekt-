using DateAndTime.Api.Contracts.Auth;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DateAndTime.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(
    AppDbContext db,
    PasswordHasher hasher,
    JwtTokenService tokens,
    IConfiguration configuration,
    IHostEnvironment environment) : ControllerBase
{
    private bool ClientAuthEnabled => environment.IsDevelopment() || environment.IsEnvironment("Testing") || configuration.GetValue<bool>("Security:EnableClientAuth");

    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ClientAuthEnabled) return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return Conflict(new ProblemDetails { Title = "An account with this email already exists." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = hasher.Hash(request.Password),
            Role = UserRole.Client,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, CreateResponse(user));
    }

    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant() && x.IsActive, cancellationToken);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash)) return Unauthorized();
        if (user.Role == UserRole.Client && !ClientAuthEnabled) return Unauthorized();
        return Ok(CreateResponse(user));
    }

    private LoginResponse CreateResponse(User user)
    {
        var (token, expires) = tokens.Create(user);
        return new LoginResponse(token, expires, user.DisplayName, user.Role.ToString());
    }
}
