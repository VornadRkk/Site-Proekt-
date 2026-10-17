using DateAndTime.Api.Contracts.Portfolio;
using DateAndTime.Api.Data;
using DateAndTime.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DateAndTime.Api.Controllers;

[ApiController, Route("api/portfolio")]
public sealed class PortfolioController(AppDbContext db) : ControllerBase
{
    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin(CancellationToken ct) => Ok(await db.PortfolioItems.AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await db.PortfolioItems.AsNoTracking().Where(x => x.IsPublished).OrderBy(x => x.DisplayOrder).Select(x => new PortfolioItemResponse(x.Id, x.Title, x.Type, x.Description, x.ImageUrl, x.DisplayOrder)).ToListAsync(ct));

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(UpsertPortfolioItemRequest request, CancellationToken ct)
    {
        var item = new PortfolioItem { Id = Guid.NewGuid(), Title = request.Title.Trim(), Type = request.Type.Trim(), Description = request.Description.Trim(), ImageUrl = request.ImageUrl, DisplayOrder = request.DisplayOrder, IsPublished = request.IsPublished };
        db.PortfolioItems.Add(item); await db.SaveChangesAsync(ct);
        return Created($"/api/portfolio/{item.Id}", new PortfolioItemResponse(item.Id, item.Title, item.Type, item.Description, item.ImageUrl, item.DisplayOrder));
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpsertPortfolioItemRequest request, CancellationToken ct)
    {
        var item = await db.PortfolioItems.FindAsync([id], ct); if (item is null) return NotFound();
        item.Title = request.Title.Trim(); item.Type = request.Type.Trim(); item.Description = request.Description.Trim(); item.ImageUrl = request.ImageUrl; item.DisplayOrder = request.DisplayOrder; item.IsPublished = request.IsPublished;
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var item = await db.PortfolioItems.FindAsync([id], ct); if (item is null) return NotFound();
        item.IsPublished = false; await db.SaveChangesAsync(ct); return NoContent();
    }
}
