using DateAndTime.Api.Contracts.Auth;
using DateAndTime.Api.Data;
using DateAndTime.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace DateAndTime.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(AppDbContext db, PasswordHasher hasher, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("login"), EnableRateLimiting("admin-login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant() && x.IsActive, cancellationToken);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash)) return Unauthorized();
        var (token, expires) = tokens.Create(user);
        return Ok(new LoginResponse(token, expires, user.DisplayName, user.Role.ToString()));
    }
}
