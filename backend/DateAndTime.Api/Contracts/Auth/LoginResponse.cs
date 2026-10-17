namespace DateAndTime.Api.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string DisplayName, string Role);
