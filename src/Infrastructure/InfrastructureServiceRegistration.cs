using AICalendar.Application.Common.Services;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.Services;
using AICalendar.Infrastructure.Persistence;
using AICalendar.Infrastructure.Repositories;
using AICalendar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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
                
            // Suppress the pending model changes warning in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });
        
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        
        // Register domain services
        services.AddScoped<ISchedulingService, AICalendar.Infrastructure.Services.SchedulingService>();
        
        // Register authentication services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        
        // Configure JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")))
            };
        });
        
        // Add authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
        });
        
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
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            logger.LogInformation("Checking database existence and structure");
            
            // For development, when we are still making changes to the model
            // Just ensure database exists and create it without migrations if needed
            await context.Database.EnsureCreatedAsync();
            
            // Skip migration and just use EnsureCreated in development for now
            // When ready for production, uncomment the migration line below
            // await context.Database.MigrateAsync();
            
            // Seed data
            logger.LogInformation("Seeding database");
            var seeder = services.GetRequiredService<ApplicationDbSeeder>();
            await seeder.SeedAsync();
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}