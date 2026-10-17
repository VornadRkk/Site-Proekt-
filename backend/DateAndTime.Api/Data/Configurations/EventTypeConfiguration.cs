using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DateAndTime.Api.Data.Configurations;

public sealed class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("event_types");
        builder.HasKey(type => type.Id);
        builder.Property(type => type.Code).HasMaxLength(32).IsRequired();
        builder.HasIndex(type => type.Code).IsUnique();
        builder.Property(type => type.Name).HasMaxLength(120).IsRequired();
    }
}
