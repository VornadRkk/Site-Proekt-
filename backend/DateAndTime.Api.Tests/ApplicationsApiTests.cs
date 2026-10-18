using DateAndTime.Api.Contracts.Applications;
using DateAndTime.Api.Controllers;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
            Comment = "Игровая программа"
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
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        var actionResult = await controller.Create(request, CancellationToken.None);

        var validationResult = Assert.IsType<ObjectResult>(actionResult.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(validationResult.Value);
        Assert.Contains(nameof(request.EventDate), problem.Errors.Keys);
        Assert.Empty(dbContext.Applications);
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyApplicationsForAuthenticatedUser()
    {
        await using var dbContext = TestDbContext.Create();
        var clientId = Guid.NewGuid();
        dbContext.Applications.AddRange(
            CreateSavedApplication("client@example.com", clientId),
            CreateSavedApplication("other@example.com", Guid.NewGuid()),
            CreateSavedApplication("client@example.com", null));
        await dbContext.SaveChangesAsync();
        var controller = new ApplicationsController(dbContext)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                        [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, clientId.ToString())],
                        "test"))
                }
            }
        };

        var result = await controller.GetMine(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var applications = Assert.IsAssignableFrom<IReadOnlyList<ApplicationResponse>>(ok.Value);
        var application = Assert.Single(applications);
        Assert.Equal("client@example.com", application.Email);
    }

    private static ApplicationRequest CreateSavedApplication(string email, Guid? userId) => new()
    {
        Id = Guid.NewGuid(),
        ClientName = "Клиент",
        Phone = "+79990000000",
        Email = email,
        EventType = ServiceCategory.Celebration,
        EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        GuestCount = 0,
        UserId = userId,
        Status = ApplicationStatus.New,
        CreatedAtUtc = DateTime.UtcNow
    };
}
