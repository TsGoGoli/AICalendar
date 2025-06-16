using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AICalendar.Infrastructure.Persistence;

/// <summary>
/// Factory to create ApplicationDbContext for design-time operations like migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration 
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Use a hardcoded connection string if appsettings.json isn't found
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
            "Server=(localdb)\\mssqllocaldb;Database=AICalendarDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}