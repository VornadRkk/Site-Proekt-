namespace DateAndTime.Api.Domain;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Admin;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}

public enum UserRole
{
    Client = 0,
    Admin = 1
}
