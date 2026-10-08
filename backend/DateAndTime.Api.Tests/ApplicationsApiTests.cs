using DateAndTime.Api.Contracts.Applications;
using DateAndTime.Api.Controllers;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DateAndTime.Api.Tests;

public sealed class ApplicationsApiTests
{
    [Fact]
    public async Task CreateApplication_WithValidRequest_PersistsApplication()
    {
        await using var dbContext = TestDbContext.Create();
        var controller = new ApplicationsController(dbContext);
        var request = new CreateApplicationRequest
        {
            ClientName = "Анна",
            Phone = "+7 999 123-45-67",
            Email = "Anna@example.com",
            EventType = ServiceCategory.Theatre,
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            GuestCount = 50,
            Comment = "Интерактивная постановка"
        };

        var actionResult = await controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var created = Assert.IsType<ApplicationCreatedResponse>(createdResult.Value);
        Assert.Equal(ApplicationStatus.New, created.Status);

        var saved = Assert.Single(dbContext.Applications);
        Assert.Equal("anna@example.com", saved.Email);
    }

    [Fact]
    public async Task CreateApplication_WithPastDate_ReturnsValidationProblem()
    {
        await using var dbContext = TestDbContext.Create();
        var controller = new ApplicationsController(dbContext);
        var request = new CreateApplicationRequest
        {
            ClientName = "Иван",
            Phone = "+7 999 000-00-00",
            Email = "ivan@example.com",
            EventType = ServiceCategory.Celebration,
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            GuestCount = 20
        };

        var actionResult = await controller.Create(request, CancellationToken.None);

        var validationResult = Assert.IsType<ObjectResult>(actionResult.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(validationResult.Value);
        Assert.Contains(nameof(request.EventDate), problem.Errors.Keys);
        Assert.Empty(dbContext.Applications);
    }
}
