using System.ComponentModel.DataAnnotations;

namespace DateAndTime.Api.Contracts.Portfolio;

public sealed class UpsertPortfolioItemRequest
{
    [Required, MaxLength(160)] public required string Title { get; init; }
    [Required, MaxLength(120)] public required string Type { get; init; }
    [Required, MaxLength(2000)] public required string Description { get; init; }
    [MaxLength(500)] public string? ImageUrl { get; init; }
    [Range(0, 10000)] public int DisplayOrder { get; init; }
    public bool IsPublished { get; init; } = true;
}
