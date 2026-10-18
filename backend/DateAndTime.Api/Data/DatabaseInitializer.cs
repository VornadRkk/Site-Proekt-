using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DateAndTime.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db, IConfiguration configuration, PasswordHasher hasher)
    {
        await EnsureApplicationOwnerColumnAsync(db);
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `users` (`Id` char(36) NOT NULL, `Email` varchar(254) NOT NULL, `DisplayName` varchar(120) NOT NULL, `PasswordHash` varchar(300) NOT NULL, `Role` varchar(32) NOT NULL, `IsActive` tinyint(1) NOT NULL, `CreatedAtUtc` datetime(6) NOT NULL, CONSTRAINT `PK_users` PRIMARY KEY (`Id`), CONSTRAINT `IX_users_Email` UNIQUE (`Email`)) CHARACTER SET utf8mb4");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `portfolio_items` (`Id` char(36) NOT NULL, `Title` varchar(160) NOT NULL, `Type` varchar(120) NOT NULL, `Description` varchar(2000) NOT NULL, `ImageUrl` varchar(500) NULL, `DisplayOrder` int NOT NULL, `IsPublished` tinyint(1) NOT NULL, CONSTRAINT `PK_portfolio_items` PRIMARY KEY (`Id`)) CHARACTER SET utf8mb4");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `event_types` (`Id` char(36) NOT NULL, `Code` varchar(32) NOT NULL, `Name` varchar(120) NOT NULL, `IsActive` tinyint(1) NOT NULL, CONSTRAINT `PK_event_types` PRIMARY KEY (`Id`), CONSTRAINT `IX_event_types_Code` UNIQUE (`Code`)) CHARACTER SET utf8mb4");

        var eventTypes = new[]
        {
            ("celebration", "Праздник"),
            ("theatre", "Театрализованная программа"),
            ("festival", "Фестиваль"),
            ("birthday", "Частный праздник"),
            ("wedding", "Свадьба"),
            ("corporate", "Корпоратив"),
            ("other", "Другое")
        };
        foreach (var (code, name) in eventTypes)
        {
            var eventType = await db.EventTypes.SingleOrDefaultAsync(item => item.Code == code);
            if (eventType is null) db.EventTypes.Add(new EventType { Id = Guid.NewGuid(), Code = code, Name = name });
            else { eventType.Name = name; eventType.IsActive = true; }
        }

        var serviceDefinitions = new[]
        {
            new Service { Id = new Guid("88a41a6d-f625-4a32-a3b5-0727c888e312"), Name = "Игровые программы", ShortDescription = "Живые игровые форматы для детей и взрослых с ведущими, персонажами и общей историей.", PriceFrom = 45000, Category = ServiceCategory.Celebration, IsActive = true, DisplayOrder = 3 },
            new Service { Id = new Guid("752c9775-4c91-4d52-ab09-8515fb41b642"), Name = "Частные праздники", ShortDescription = "Личный сценарий для семейного события, камерного вечера или большой вечеринки.", PriceFrom = 75000, Category = ServiceCategory.Birthday, IsActive = true, DisplayOrder = 4 },
            new Service { Id = new Guid("ab32a0d5-7eb7-4207-9d9b-e3fc5f750dee"), Name = "Фестивальные программы", ShortDescription = "Концепция, сценарий и творческое наполнение для городских и тематических фестивалей.", PriceFrom = 150000, Category = ServiceCategory.Festival, IsActive = true, DisplayOrder = 7 }
        };
        foreach (var definition in serviceDefinitions)
        {
            var service = await db.Services.SingleOrDefaultAsync(item => item.Id == definition.Id);
            if (service is null) db.Services.Add(definition);
            else
            {
                service.Name = definition.Name;
                service.ShortDescription = definition.ShortDescription;
                service.PriceFrom = definition.PriceFrom;
                service.Category = definition.Category;
                service.IsActive = true;
                service.DisplayOrder = definition.DisplayOrder;
            }
        }
        await db.SaveChangesAsync();

        if (!await db.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            var email = configuration["Admin:Email"] ?? "admin@dateandtime.local";
            var password = configuration["Admin:Password"];
            if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Admin:Password must be configured before the first API start.");
            db.Users.Add(new User { Id = Guid.NewGuid(), Email = email.Trim().ToLowerInvariant(), DisplayName = "Администратор", PasswordHash = hasher.Hash(password), Role = UserRole.Admin, CreatedAtUtc = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureApplicationOwnerColumnAsync(AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        var closeAfter = connection.State == ConnectionState.Closed;
        if (closeAfter) await connection.OpenAsync();
        try
        {
            await using var columnCommand = connection.CreateCommand();
            columnCommand.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'applications' AND COLUMN_NAME = 'UserId'";
            if (Convert.ToInt32(await columnCommand.ExecuteScalarAsync()) == 0)
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE `applications` ADD COLUMN `UserId` char(36) NULL COLLATE ascii_general_ci");

            await using var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = "SELECT COUNT(*) FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'applications' AND INDEX_NAME = 'IX_applications_UserId'";
            if (Convert.ToInt32(await indexCommand.ExecuteScalarAsync()) == 0)
                await db.Database.ExecuteSqlRawAsync("CREATE INDEX `IX_applications_UserId` ON `applications` (`UserId`)");
        }
        finally
        {
            if (closeAfter) await connection.CloseAsync();
        }
    }
}
