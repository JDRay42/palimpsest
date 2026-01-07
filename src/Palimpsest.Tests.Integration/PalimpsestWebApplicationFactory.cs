using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Tests.Integration;

/// <summary>
/// Custom web application factory for integration tests.
/// Configures the test app to use an in-memory database.
/// </summary>
public class PalimpsestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database context registration
            services.RemoveAll(typeof(DbContextOptions<PalimpsestDbContext>));
            
            // Add an in-memory database for testing  
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PalimpsestDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<PalimpsestDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider and create the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<PalimpsestDbContext>();

            db.Database.EnsureCreated();
        });
    }
}
