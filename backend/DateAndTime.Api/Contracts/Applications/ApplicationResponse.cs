using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Applications;

public sealed record ApplicationResponse(
    Guid Id,
    string ClientName,
    string Phone,
    string Email,
    ServiceCategory EventType,
    DateOnly EventDate,
    string? Comment,
    ApplicationStatus Status,
    DateTime CreatedAtUtc);
