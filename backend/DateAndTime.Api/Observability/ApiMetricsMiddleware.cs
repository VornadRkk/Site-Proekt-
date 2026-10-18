using System.Diagnostics;

namespace DateAndTime.Api.Observability;

public sealed class ApiMetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApiMetrics metrics)
    {
        var started = Stopwatch.GetTimestamp();
        try { await next(context); }
        finally
        {
            if (!context.Request.Path.StartsWithSegments("/metrics"))
                metrics.Observe(context.Request.Method, context.Request.Path, context.Response.StatusCode, Stopwatch.GetElapsedTime(started));
        }
    }
}
