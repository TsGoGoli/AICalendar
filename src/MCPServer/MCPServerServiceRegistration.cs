using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AICalendar.MCPServer;

/// <summary>
/// Extension methods for registering MCP Server services
/// </summary>
public static class MCPServerServiceRegistration
{
    public static IServiceCollection AddMCPServer(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HTTP client for API communication
        services.AddHttpClient<CalendarApiClient>(client =>
        {
            var apiBaseUrl = configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7001";
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "AICalendar-MCPServer/1.0.0");
        });

        // Add the main MCP server
        services.AddSingleton<CalendarMCPServer>();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }
}
