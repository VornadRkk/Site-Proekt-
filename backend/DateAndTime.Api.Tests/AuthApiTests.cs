using DateAndTime.Api.Contracts.Auth;
using DateAndTime.Api.Controllers;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DateAndTime.Api.Tests;

public sealed class AuthApiTests
{
    [Fact]
    public async Task Register_AssignsClientRoleAndHashesPassword()
    {
        await using var db = TestDbContext.Create();
        var controller = CreateController(db);

        var result = await controller.Register(new RegisterRequest
        {
            DisplayName = "Анна",
            Email = "Anna@Example.com",
            Password = "StrongPassword123!"
        }, CancellationToken.None);

        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        var response = Assert.IsType<LoginResponse>(created.Value);
        Assert.Equal("Client", response.Role);
        var user = Assert.Single(db.Users);
        Assert.Equal(UserRole.Client, user.Role);
        Assert.Equal("anna@example.com", user.Email);
        Assert.DoesNotContain("StrongPassword123!", user.PasswordHash);
        Assert.True(new PasswordHasher().Verify("StrongPassword123!", user.PasswordHash));
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmailIgnoringCase()
    {
        await using var db = TestDbContext.Create();
        var hasher = new PasswordHasher();
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "client@example.com", DisplayName = "Client", PasswordHash = hasher.Hash("StrongPassword123!"), Role = UserRole.Client, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = CreateController(db);

        var result = await controller.Register(new RegisterRequest { DisplayName = "Другой", Email = "CLIENT@example.com", Password = "AnotherPassword123!" }, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Login_ReturnsSameUnauthorizedResultForUnknownEmailAndWrongPassword()
    {
        await using var db = TestDbContext.Create();
        var hasher = new PasswordHasher();
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "client@example.com", DisplayName = "Client", PasswordHash = hasher.Hash("StrongPassword123!"), Role = UserRole.Client, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = CreateController(db);

        var unknown = await controller.Login(new LoginRequest { Email = "missing@example.com", Password = "WrongPassword123!" }, CancellationToken.None);
        var wrong = await controller.Login(new LoginRequest { Email = "client@example.com", Password = "WrongPassword123!" }, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(unknown.Result);
        Assert.IsType<UnauthorizedResult>(wrong.Result);
    }

    private static AuthController CreateController(AppDbContext db)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-only-jwt-signing-key-with-at-least-32-bytes",
            ["Jwt:LifetimeHours"] = "1",
            ["Security:EnableClientAuth"] = "true"
        }).Build();
        var hasher = new PasswordHasher();
        return new AuthController(db, hasher, new JwtTokenService(configuration), configuration, new TestHostEnvironment());
    }
}
