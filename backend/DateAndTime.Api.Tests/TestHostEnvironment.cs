using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DateAndTime.Api.Tests;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Testing";
    public string ApplicationName { get; set; } = "DateAndTime.Api.Tests";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
