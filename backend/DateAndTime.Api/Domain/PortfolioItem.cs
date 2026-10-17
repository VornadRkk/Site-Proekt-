namespace DateAndTime.Api.Domain;

public sealed class PortfolioItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}
