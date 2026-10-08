using System.ComponentModel.DataAnnotations;
using DateAndTime.Api.Domain;

namespace DateAndTime.Api.Contracts.Applications;

public sealed class CreateApplicationRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public required string ClientName { get; init; }

    [Required, StringLength(30, MinimumLength = 6)]
    [RegularExpression(@"^[0-9+()\-\s]+$", ErrorMessage = "Phone contains unsupported characters.")]
    public required string Phone { get; init; }

    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [Required]
    public ServiceCategory? EventType { get; init; }

    [Required]
    public DateOnly? EventDate { get; init; }

    [Range(1, 5000)]
    public int GuestCount { get; init; }

    [StringLength(2000)]
    public string? Comment { get; init; }
}
