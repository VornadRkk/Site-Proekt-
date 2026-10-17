namespace DateAndTime.Api.Contracts.Portfolio;

public sealed record PortfolioItemResponse(Guid Id, string Title, string Type, string Description, string? ImageUrl, int DisplayOrder);
