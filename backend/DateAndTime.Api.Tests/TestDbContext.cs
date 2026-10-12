using DateAndTime.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DateAndTime.Api.Tests;

internal static class TestDbContext
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"date-and-time-tests-{Guid.NewGuid()}")
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
