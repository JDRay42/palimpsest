using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Palimpsest.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by dotnet ef commands to create migrations.
/// </summary>
public class PalimpsestDbContextFactory : IDesignTimeDbContextFactory<PalimpsestDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="PalimpsestDbContext"/> for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A configured <see cref="PalimpsestDbContext"/> instance.</returns>
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
