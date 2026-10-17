using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DateAndTime.Api.Domain;
using Microsoft.IdentityModel.Tokens;

namespace DateAndTime.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiresAtUtc) Create(User user)
    {
        var expires = DateTime.UtcNow.AddHours(configuration.GetValue("Jwt:LifetimeHours", 8));
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
