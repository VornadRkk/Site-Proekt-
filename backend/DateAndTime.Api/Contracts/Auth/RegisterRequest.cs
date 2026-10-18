using System.ComponentModel.DataAnnotations;

namespace DateAndTime.Api.Contracts.Auth;

public sealed class RegisterRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public required string DisplayName { get; init; }

    [Required, EmailAddress, MaxLength(254)]
    public required string Email { get; init; }

    [Required, MinLength(12), MaxLength(128)]
    public required string Password { get; init; }
}
