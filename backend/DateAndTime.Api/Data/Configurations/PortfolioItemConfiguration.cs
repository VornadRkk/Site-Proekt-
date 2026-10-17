using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DateAndTime.Api.Data.Configurations;

public sealed class PortfolioItemConfiguration : IEntityTypeConfiguration<PortfolioItem>
{
    public void Configure(EntityTypeBuilder<PortfolioItem> builder)
    {
        builder.ToTable("portfolio_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Title).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Type).HasMaxLength(120).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.ImageUrl).HasMaxLength(500);
        builder.HasIndex(item => new { item.IsPublished, item.DisplayOrder });
    }
}
