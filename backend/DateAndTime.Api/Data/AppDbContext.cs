using DateAndTime.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DateAndTime.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ApplicationRequest> Applications => Set<ApplicationRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<EventType> EventTypes => Set<EventType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
