using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Services;

public sealed record ServiceResponse(
    Guid Id,
    string Name,
    string ShortDescription,
    decimal PriceFrom,
    ServiceCategory Category);
