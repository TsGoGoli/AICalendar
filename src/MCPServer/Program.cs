using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AICalendar.MCPServer.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddScoped<CalendarApiClient>();
builder.Services.AddScoped<McpServerService>();
builder.Services.AddScoped<McpProtocolHandler>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MCP Server starting...");

// Start MCP protocol handler
var protocolHandler = host.Services.GetRequiredService<McpProtocolHandler>();
await protocolHandler.RunAsync();
