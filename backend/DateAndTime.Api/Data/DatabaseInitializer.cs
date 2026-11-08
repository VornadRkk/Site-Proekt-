using DateAndTime.Api.Domain;
using DateAndTime.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DateAndTime.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db, IConfiguration configuration, PasswordHasher hasher)
    {

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
