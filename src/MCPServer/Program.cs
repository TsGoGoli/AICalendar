using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddLogging();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MCP Server starting...");

// TODO: Add actual MCP protocol handling (stdin/stdout communication)
logger.LogInformation("MCP Server ready to accept requests");

// Keep the application running
await Task.Delay(-1);
