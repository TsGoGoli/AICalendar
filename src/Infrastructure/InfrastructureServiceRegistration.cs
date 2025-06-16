using AICalendar.Domain.Repositories;
using AICalendar.Domain.Services;
using AICalendar.Infrastructure.Persistence;
using AICalendar.Infrastructure.Repositories;
using AICalendar.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AICalendar.Infrastructure;

/// <summary>
/// Extension methods to register infrastructure services
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds infrastructure services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure logging with Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
            
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));
            
        // Configure DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });
        
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        
        // Register services - use full namespace to avoid ambiguity
        services.AddScoped<ISchedulingService, AICalendar.Infrastructure.Services.SchedulingService>();
        
        // Register DbSeeder
        services.AddScoped<ApplicationDbSeeder>();
        
        return services;
    }
    
    /// <summary>
    /// Ensures the database is created and migrated
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Apply migrations
            await context.Database.MigrateAsync();
            
            // Seed data
            var seeder = services.GetRequiredService<ApplicationDbSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}