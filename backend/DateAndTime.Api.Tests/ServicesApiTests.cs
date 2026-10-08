using DateAndTime.Api.Contracts.Services;
using DateAndTime.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DateAndTime.Api.Tests;

public sealed class ServicesApiTests
{
    [Fact]
    public async Task GetServices_ReturnsSeededActiveServices()
    {
        await using var dbContext = TestDbContext.Create();
        var controller = new ServicesController(dbContext);

        var actionResult = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var services = Assert.IsAssignableFrom<IReadOnlyList<ServiceResponse>>(okResult.Value);
        Assert.Equal(6, services.Count);
        Assert.Equal("Театрализованная программа", services[0].Name);
    }
}
