using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace DateAndTime.Api.Tests;
public sealed class ApiFactory : WebApplicationFactory<Program> {
 readonly string database = Guid.NewGuid().ToString();
 protected override void ConfigureWebHost(IWebHostBuilder builder) {
  builder.UseEnvironment("Testing");
  builder.UseSetting("Jwt:Key", "integration-tests-only-signing-key-1234567890");
  builder.UseSetting("Jwt:Key", "integration-tests-only-signing-key-1234567890");
  builder.ConfigureAppConfiguration((_,config)=>config.AddInMemoryCollection(new Dictionary<string,string?> { ["Jwt:Key"] = "integration-tests-only-signing-key-1234567890" }));
  builder.ConfigureServices(services=>{services.RemoveAll<DbContextOptions<AppDbContext>>();services.AddDbContext<AppDbContext>(o=>o.UseInMemoryDatabase(database));});
 }
}
public sealed class AuthIntegrationTests {
 [Fact] public async Task RegistrationRolesAndApplicationIsolation() {
  await using var factory=new ApiFactory(); using var client=factory.CreateClient();
  var email="First@Example.test";
  var registration=await client.PostAsJsonAsync("/api/auth/register",new {email,password="A-long-password-123",displayName="First",role="Admin"});
  registration.EnsureSuccessStatusCode();
  var auth=await registration.Content.ReadFromJsonAsync<JsonElement>();
  Assert.Equal("Client",auth.GetProperty("role").GetString());
  var duplicate=await client.PostAsJsonAsync("/api/auth/register",new {email=email.ToLowerInvariant(),password="A-long-password-123",displayName="Other"}); Assert.Equal(HttpStatusCode.Conflict,duplicate.StatusCode);
  using(var scope=factory.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();var user=await db.Users.SingleAsync();Assert.NotEqual("A-long-password-123",user.PasswordHash);Assert.True(new PasswordHasher().Verify("A-long-password-123",user.PasswordHash));}
  client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",auth.GetProperty("accessToken").GetString());
  Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/applications")).StatusCode);
  Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/services/admin")).StatusCode);
  (await client.PostAsJsonAsync("/api/applications",new {clientName="First",phone="+79991234567",email="different@example.test",eventType="birthday",eventDate=DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),guestCount=10})).EnsureSuccessStatusCode();
  var mine=await client.GetFromJsonAsync<JsonElement[]>("/api/applications/mine"); Assert.Single(mine!);
  var second=await client.PostAsJsonAsync("/api/auth/register",new {email="second@example.test",password="A-long-password-123",displayName="Second"});second.EnsureSuccessStatusCode();var other=await second.Content.ReadFromJsonAsync<JsonElement>();
  client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",other.GetProperty("accessToken").GetString());
  Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/applications/mine"))!);
  client.DefaultRequestHeaders.Authorization=null;
  Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/applications")).StatusCode);
  Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/applications/mine")).StatusCode);
 }
 [Fact] public async Task LoginChecksPasswordAndAdminSession() {
  await using var factory=new ApiFactory();using var client=factory.CreateClient();
  using(var scope=factory.Services.CreateScope()) {var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();db.Users.Add(new User {Id=Guid.NewGuid(),Email="admin@example.test",DisplayName="Admin",PasswordHash=new PasswordHasher().Hash("Long-admin-password-123"),Role=UserRole.Admin,CreatedAtUtc=DateTime.UtcNow});await db.SaveChangesAsync();}
  var missing=await client.PostAsJsonAsync("/api/auth/login",new {email="missing@example.test",password="Incorrect-password"});
  var incorrect=await client.PostAsJsonAsync("/api/auth/login",new {email="admin@example.test",password="Incorrect-password"});
  Assert.Equal(HttpStatusCode.Unauthorized,missing.StatusCode);Assert.Equal(missing.StatusCode,incorrect.StatusCode);
  var login=await client.PostAsJsonAsync("/api/auth/login",new {email="ADMIN@example.test",password="Long-admin-password-123"});login.EnsureSuccessStatusCode();
  var auth=await login.Content.ReadFromJsonAsync<JsonElement>();client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",auth.GetProperty("accessToken").GetString());
  Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/applications")).StatusCode);
  using(var scope=factory.Services.CreateScope()) {var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();(await db.Users.SingleAsync()).IsActive=false;await db.SaveChangesAsync();}
  Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/session")).StatusCode);
 }
 [Fact] public async Task RejectsShortPasswordsAndLimitsLogin() {
  await using var factory=new ApiFactory();using var client=factory.CreateClient();
  Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/auth/register",new {email="test@example.test",password="short",displayName="Test"})).StatusCode);
  for(var i=0;i<4;i++) Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {email="unknown@example.test",password="Wrong-password-123"})).StatusCode);
  Assert.Equal(HttpStatusCode.TooManyRequests,(await client.PostAsJsonAsync("/api/auth/login",new {email="unknown@example.test",password="Wrong-password-123"})).StatusCode);
 }
}
