namespace DateAndTime.Api.Domain;

public sealed class EventType
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
