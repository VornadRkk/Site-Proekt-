using System.ComponentModel.DataAnnotations;
using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Services;

public sealed class UpsertServiceRequest
{
    [Required, MaxLength(120)] public required string Name { get; init; }
    [Required, MaxLength(500)] public required string ShortDescription { get; init; }
    [Range(0, 100000000)] public decimal PriceFrom { get; init; }
    public ServiceCategory Category { get; init; }
    public bool IsActive { get; init; } = true;
    [Range(0, 10000)] public int DisplayOrder { get; init; }
}
