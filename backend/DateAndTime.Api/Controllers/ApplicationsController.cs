using DateAndTime.Api.Contracts.Applications;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        var authenticatedEmail = HttpContext?.User.Identity?.IsAuthenticated == true
            ? HttpContext.User.FindFirstValue(ClaimTypes.Email)
            : null;
        var authenticatedUserId = Guid.TryParse(HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : (Guid?)null;
        var application = new ApplicationRequest
        {
            Id = Guid.NewGuid(),
            ClientName = request.ClientName.Trim(),
            Phone = request.Phone.Trim(),
            Email = (authenticatedEmail ?? request.Email).Trim().ToLowerInvariant(),
            EventType = request.EventType!.Value,
            EventDate = request.EventDate!.Value,
            GuestCount = 0,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            UserId = authenticatedUserId,
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

    [HttpGet("mine"), Authorize(Roles = "Client")]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();

        var applications = await dbContext.Applications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => new ApplicationResponse(
                item.Id,
                item.ClientName,
                item.Phone,
                item.Email,
                item.EventType,
                item.EventDate,
                item.Comment,
                item.Status,
                item.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(applications);
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await dbContext.Applications.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken));

    [HttpPatch("{id:guid}/status"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        var item = await dbContext.Applications.FindAsync([id], cancellationToken);
        if (item is null) return NotFound();
        item.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Applications.FindAsync([id], cancellationToken);
        if (item is null) return NotFound();
        dbContext.Applications.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
