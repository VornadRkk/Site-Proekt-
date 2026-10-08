using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DateAndTime.Api.Data.Configurations;

public sealed class ApplicationRequestConfiguration : IEntityTypeConfiguration<ApplicationRequest>
{
    public void Configure(EntityTypeBuilder<ApplicationRequest> builder)
    {
        builder.ToTable("applications");
        builder.HasKey(application => application.Id);
        builder.Property(application => application.ClientName).HasMaxLength(100).IsRequired();
        builder.Property(application => application.Phone).HasMaxLength(30).IsRequired();
        builder.Property(application => application.Email).HasMaxLength(254).IsRequired();
        builder.Property(application => application.EventType).HasConversion<string>().HasMaxLength(32);
        builder.Property(application => application.Comment).HasMaxLength(2000);
        builder.Property(application => application.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(application => application.CreatedAtUtc);
        builder.HasIndex(application => application.Status);
    }
}
