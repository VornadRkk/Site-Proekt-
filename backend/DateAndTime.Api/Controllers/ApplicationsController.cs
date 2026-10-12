using DateAndTime.Api.Contracts.Applications;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DateAndTime.Api.Controllers;

[ApiController]
[Route("api/applications")]
[EnableRateLimiting("applications")]
public sealed class ApplicationsController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ApplicationCreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApplicationCreatedResponse>> Create(
        CreateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            ModelState.AddModelError(nameof(request.EventDate), "Event date cannot be in the past.");
            return ValidationProblem(ModelState);
        }

        var application = new ApplicationRequest
        {
            Id = Guid.NewGuid(),
            ClientName = request.ClientName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            EventType = request.EventType!.Value,
            EventDate = request.EventDate!.Value,
            GuestCount = request.GuestCount,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            Status = ApplicationStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Applications.Add(application);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ApplicationCreatedResponse(
            application.Id,
            application.Status,
            application.CreatedAtUtc);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
