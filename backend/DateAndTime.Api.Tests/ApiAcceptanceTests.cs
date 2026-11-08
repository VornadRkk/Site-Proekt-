using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DateAndTime.Api.Tests;

public sealed class ApiAcceptanceTests
{
    private const string ItemId = "00000000-0000-0000-0000-000000000123";

    [Theory]
    [InlineData("GET", "/api/services/admin")]
    [InlineData("GET", "/api/portfolio/admin")]
    [InlineData("GET", "/api/applications")]
    [InlineData("POST", "/api/services")]
    [InlineData("POST", "/api/portfolio")]
    [InlineData("PUT", "/api/services/" + ItemId)]
    [InlineData("PUT", "/api/portfolio/" + ItemId)]
    [InlineData("DELETE", "/api/services/" + ItemId)]
    [InlineData("DELETE", "/api/portfolio/" + ItemId)]
    [InlineData("DELETE", "/api/applications/" + ItemId)]
    [InlineData("PATCH", "/api/applications/" + ItemId + "/status")]
    public async Task EveryAdministrativeOperationRejectsAnonymousClientAndInvalidTokens(string method, string path)
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var anonymous = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await CreateUserToken(factory, UserRole.Client));
        using var forbidden = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
        using var invalid = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
    }

    [Fact]
    public async Task ApplicationStatusChangesAreVisibleToOwnerAndDeletionRemovesIt()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var clientToken = await CreateUserToken(factory, UserRole.Client);
        var adminToken = await CreateUserToken(factory, UserRole.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
        using var created = await client.PostAsJsonAsync("/api/applications", ValidApplication());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = item.GetProperty("id").GetGuid();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        using var updated = await client.PatchAsJsonAsync($"/api/applications/{id}/status", new { status = "completed" });
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
        var mine = await client.GetFromJsonAsync<JsonElement[]>("/api/applications/mine");
        Assert.Equal("completed", Assert.Single(mine!).GetProperty("status").GetString());

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        using var missing = await client.PatchAsJsonAsync($"/api/applications/{Guid.NewGuid()}/status", new { status = "completed" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var deleted = await client.DeleteAsync($"/api/applications/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
        Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/applications/mine"))!);
    }

    [Fact]
    public async Task AnonymousApplicationIsAcceptedAndInvalidDataIsRejected()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var invalid = await client.PostAsJsonAsync("/api/applications", new { clientName = "A", phone = "bad", email = "bad", eventType = "birthday", eventDate = "2000-01-01", guestCount = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var created = await client.PostAsJsonAsync("/api/applications", ValidApplication());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var scope = factory.Services.CreateScope();
        var saved = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Applications.SingleAsync();
        Assert.Null(saved.UserId);
        Assert.Equal(ApplicationStatus.New, saved.Status);
    }

    private static object ValidApplication() => new
    {
        clientName = "API acceptance", phone = "+79991234567", email = "request@example.test",
        eventType = "birthday", eventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), guestCount = 10
    };

    private static async Task<string> CreateUserToken(ApiFactory factory, UserRole role)
    {
        using var scope = factory.Services.CreateScope();
        var user = new User
        {
            Id = Guid.NewGuid(), Email = $"{Guid.NewGuid()}@example.test", DisplayName = role.ToString(),
            PasswordHash = new PasswordHasher().Hash(Guid.NewGuid().ToString()), Role = role, CreatedAtUtc = DateTime.UtcNow
        };
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return scope.ServiceProvider.GetRequiredService<JwtTokenService>().Create(user).Token;
    }
}
