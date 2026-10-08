using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DateAndTime.Api.Data.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(service => service.Id);
        builder.Property(service => service.Name).HasMaxLength(120).IsRequired();
        builder.Property(service => service.ShortDescription).HasMaxLength(500).IsRequired();
        builder.Property(service => service.PriceFrom).HasPrecision(12, 2);
        builder.Property(service => service.Category).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(service => service.DisplayOrder);

        builder.HasData(
            Create(new Guid("15373850-0b42-4e3f-87aa-fb2f6477cc31"), "Театрализованная программа", "Авторский сценарий, образы и живое представление для вашей площадки и зрителей.", 60000, ServiceCategory.Theatre, 1),
            Create(new Guid("c9ddb5a8-f5d0-4180-aadf-d9304c782e34"), "Праздник под ключ", "Идея, оформление, артисты и программа, собранные в цельный праздник.", 80000, ServiceCategory.Celebration, 2),
            Create(new Guid("88a41a6d-f625-4a32-a3b5-0727c888e312"), "Интерактивный спектакль", "История, в которой зрители становятся участниками происходящего.", 45000, ServiceCategory.Theatre, 3),
            Create(new Guid("752c9775-4c91-4d52-ab09-8515fb41b642"), "День рождения", "Личный сценарий для семейного праздника или большой вечеринки.", 75000, ServiceCategory.Birthday, 4),
            Create(new Guid("b0ce73b4-a9a1-41f2-a7e3-43c2988696f3"), "Свадебная программа", "Творческие номера и тёплые детали для важного дня.", 120000, ServiceCategory.Wedding, 5),
            Create(new Guid("56ddf997-591d-419e-9c36-5acfdde51ce1"), "Корпоративный праздник", "Тематическая программа и представление для команды.", 100000, ServiceCategory.Corporate, 6));
    }

    private static Service Create(Guid id, string name, string description, decimal price, ServiceCategory category, int order) =>
        new()
        {
            Id = id,
            Name = name,
            ShortDescription = description,
            PriceFrom = price,
            Category = category,
            IsActive = true,
            DisplayOrder = order
        };
}
