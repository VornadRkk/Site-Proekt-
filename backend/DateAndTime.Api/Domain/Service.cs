namespace DateAndTime.Api.Domain;

public sealed class Service
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public decimal PriceFrom { get; set; }
    public ServiceCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
