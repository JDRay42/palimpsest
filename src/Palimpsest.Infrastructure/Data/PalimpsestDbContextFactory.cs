using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Palimpsest.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by dotnet ef commands to create migrations.
/// </summary>
public class PalimpsestDbContextFactory : IDesignTimeDbContextFactory<PalimpsestDbContext>
{
    public PalimpsestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PalimpsestDbContext>();
        
        // Use a default connection string for migrations
        // This will be overridden by appsettings.json in production
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=palimpsest;Username=palimpsest;Password=palimpsest_dev",
            npgsqlOptions => npgsqlOptions.UseVector());

        return new PalimpsestDbContext(optionsBuilder.Options);
    }
}
