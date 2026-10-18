using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace DateAndTime.Api.Observability;

public sealed class ApiMetrics
{
    private sealed class Bucket
    {
        public long Count;
        public long ElapsedTicks;
    }

    private readonly ConcurrentDictionary<(string Method, string Path, int Status), Bucket> buckets = new();

    public void Observe(string method, string path, int status, TimeSpan elapsed)
    {
        var bucket = buckets.GetOrAdd((method, NormalizePath(path), status), _ => new Bucket());
        Interlocked.Increment(ref bucket.Count);
        Interlocked.Add(ref bucket.ElapsedTicks, elapsed.Ticks);
    }

    public async Task WriteAsync(HttpResponse response, CancellationToken cancellationToken)
    {
        response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
        var output = new StringBuilder()
            .AppendLine("# HELP dateandtime_http_requests_total Total HTTP requests handled by the API.")
            .AppendLine("# TYPE dateandtime_http_requests_total counter")
            .AppendLine("# HELP dateandtime_http_request_duration_seconds Request duration grouped by method, path and status.")
            .AppendLine("# TYPE dateandtime_http_request_duration_seconds summary");
        foreach (var entry in buckets.OrderBy(item => item.Key.Method).ThenBy(item => item.Key.Path).ThenBy(item => item.Key.Status))
        {
            var labels = $"method=\"{Escape(entry.Key.Method)}\",path=\"{Escape(entry.Key.Path)}\",status=\"{entry.Key.Status}\"";
            var count = Interlocked.Read(ref entry.Value.Count);
            var seconds = TimeSpan.FromTicks(Interlocked.Read(ref entry.Value.ElapsedTicks)).TotalSeconds;
            output.Append("dateandtime_http_requests_total{").Append(labels).Append("} ").AppendLine(count.ToString(CultureInfo.InvariantCulture));
            output.Append("dateandtime_http_request_duration_seconds_sum{").Append(labels).Append("} ").AppendLine(seconds.ToString("0.#########", CultureInfo.InvariantCulture));
            output.Append("dateandtime_http_request_duration_seconds_count{").Append(labels).Append("} ").AppendLine(count.ToString(CultureInfo.InvariantCulture));
        }
        await response.WriteAsync(output.ToString(), cancellationToken);
    }

    private static string NormalizePath(string path) => string.Join('/', path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(segment => Guid.TryParse(segment, out _) ? "{id}" : segment)) is { Length: > 0 } normalized ? $"/{normalized}" : "/";
    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
}
