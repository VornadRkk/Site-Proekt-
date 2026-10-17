using DateAndTime.Api.Contracts.Services;
using DateAndTime.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await dbContext.Services
            .AsNoTracking()
            .Where(service => service.IsActive)
            .OrderBy(service => service.DisplayOrder)
            .Select(service => new ServiceResponse(
                service.Id,
                service.Name,
                service.ShortDescription,
                service.PriceFrom,
                service.Category))
            .ToListAsync(cancellationToken);

        return Ok(services);
    }

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin(CancellationToken cancellationToken) =>
        Ok(await dbContext.Services.AsNoTracking().OrderBy(service => service.DisplayOrder).ToListAsync(cancellationToken));

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceResponse>> Create(UpsertServiceRequest request, CancellationToken cancellationToken)
    {
        var item = new Service { Id = Guid.NewGuid(), Name = request.Name.Trim(), ShortDescription = request.ShortDescription.Trim(), PriceFrom = request.PriceFrom, Category = request.Category, IsActive = request.IsActive, DisplayOrder = request.DisplayOrder };
        dbContext.Services.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Created($"/api/services/{item.Id}", new ServiceResponse(item.Id, item.Name, item.ShortDescription, item.PriceFrom, item.Category));
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpsertServiceRequest request, CancellationToken cancellationToken)
    {
        var item = await dbContext.Services.FindAsync([id], cancellationToken);
        if (item is null) return NotFound();
        item.Name = request.Name.Trim(); item.ShortDescription = request.ShortDescription.Trim(); item.PriceFrom = request.PriceFrom; item.Category = request.Category; item.IsActive = request.IsActive; item.DisplayOrder = request.DisplayOrder;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Services.FindAsync([id], cancellationToken);
        if (item is null) return NotFound();
        item.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
