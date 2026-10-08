namespace DateAndTime.Api.Domain;

public sealed class ApplicationRequest
{
    public Guid Id { get; set; }
    public required string ClientName { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public ServiceCategory EventType { get; set; }
    public DateOnly EventDate { get; set; }
    public int GuestCount { get; set; }
    public string? Comment { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
    public DateTime CreatedAtUtc { get; set; }
}
