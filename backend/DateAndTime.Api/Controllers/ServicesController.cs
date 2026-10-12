using DateAndTime.Api.Contracts.Services;
using DateAndTime.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
