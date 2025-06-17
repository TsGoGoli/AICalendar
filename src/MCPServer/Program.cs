using AICalendar.MCPServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services
builder.Services.AddMCPServer(builder.Configuration);
builder.Services.AddSingleton<MCPHandler>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting AI Calendar MCP Server...");

try
{
    var handler = host.Services.GetRequiredService<MCPHandler>();
    var cancellationToken = new CancellationTokenSource();
    
    // Handle Ctrl+C gracefully
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellationToken.Cancel();
        logger.LogInformation("Shutdown requested");
    };

    await handler.RunAsync(cancellationToken.Token);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "MCP Server terminated unexpectedly");
    throw;
}
finally
{
    logger.LogInformation("AI Calendar MCP Server stopped");
}
