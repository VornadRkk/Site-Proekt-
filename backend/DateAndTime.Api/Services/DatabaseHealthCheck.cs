using DateAndTime.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace DateAndTime.Api.Services;
public sealed class DatabaseHealthCheck(IServiceScopeFactory scopes) : IHealthCheck {
 public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
  await using var scope = scopes.CreateAsyncScope();
  try { return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Database unavailable"); }
  catch { return HealthCheckResult.Unhealthy("Database unavailable"); }
 }
}
