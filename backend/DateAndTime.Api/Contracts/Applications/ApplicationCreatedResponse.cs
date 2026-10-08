using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Applications;

public sealed record ApplicationCreatedResponse(
    Guid Id,
    ApplicationStatus Status,
    DateTime CreatedAtUtc);
