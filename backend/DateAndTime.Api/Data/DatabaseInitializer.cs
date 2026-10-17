using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DateAndTime.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db, IConfiguration configuration, PasswordHasher hasher)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `users` (`Id` char(36) NOT NULL, `Email` varchar(254) NOT NULL, `DisplayName` varchar(120) NOT NULL, `PasswordHash` varchar(300) NOT NULL, `Role` varchar(32) NOT NULL, `IsActive` tinyint(1) NOT NULL, `CreatedAtUtc` datetime(6) NOT NULL, CONSTRAINT `PK_users` PRIMARY KEY (`Id`), CONSTRAINT `IX_users_Email` UNIQUE (`Email`)) CHARACTER SET utf8mb4");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `portfolio_items` (`Id` char(36) NOT NULL, `Title` varchar(160) NOT NULL, `Type` varchar(120) NOT NULL, `Description` varchar(2000) NOT NULL, `ImageUrl` varchar(500) NULL, `DisplayOrder` int NOT NULL, `IsPublished` tinyint(1) NOT NULL, CONSTRAINT `PK_portfolio_items` PRIMARY KEY (`Id`)) CHARACTER SET utf8mb4");
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS `event_types` (`Id` char(36) NOT NULL, `Code` varchar(32) NOT NULL, `Name` varchar(120) NOT NULL, `IsActive` tinyint(1) NOT NULL, CONSTRAINT `PK_event_types` PRIMARY KEY (`Id`), CONSTRAINT `IX_event_types_Code` UNIQUE (`Code`)) CHARACTER SET utf8mb4");

        if (!await db.EventTypes.AnyAsync())
        {
            db.EventTypes.AddRange(
                new EventType { Id = Guid.NewGuid(), Code = "celebration", Name = "Праздник" },
                new EventType { Id = Guid.NewGuid(), Code = "theatre", Name = "Театральная программа" },
                new EventType { Id = Guid.NewGuid(), Code = "birthday", Name = "День рождения" },
                new EventType { Id = Guid.NewGuid(), Code = "wedding", Name = "Свадьба" },
                new EventType { Id = Guid.NewGuid(), Code = "corporate", Name = "Корпоратив" },
                new EventType { Id = Guid.NewGuid(), Code = "other", Name = "Другое" });
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var email = configuration["Admin:Email"] ?? "admin@dateandtime.local";
            var password = configuration["Admin:Password"];
            if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Admin:Password must be configured before the first API start.");
            db.Users.Add(new User { Id = Guid.NewGuid(), Email = email.Trim().ToLowerInvariant(), DisplayName = "Администратор", PasswordHash = hasher.Hash(password), Role = UserRole.Admin, CreatedAtUtc = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }
}
