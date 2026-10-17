using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DateAndTime.Api.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(300).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);
    }
}
