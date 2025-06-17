using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AICalendar.MCPServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddScoped<CalendarApiClient>();
builder.Services.AddScoped<McpServerService>();
builder.Services.AddCors();

var app = builder.Build();

// Configure CORS
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("MCP Server starting as HTTP server...");

// MCP HTTP endpoints
app.MapPost("/mcp", async (HttpContext context, McpServerService mcpService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    
    logger.LogInformation("Received MCP request: {Request}", requestBody);
    
    try
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<AICalendar.MCPServer.Models.McpRequest>(requestBody);
        if (request != null)
        {
            var response = await mcpService.HandleRequestAsync(request);
            return Results.Json(response);
        }
        return Results.BadRequest("Invalid request format");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing MCP request");
        return Results.Problem("Internal server error");
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Info endpoint for MCP discovery
app.MapGet("/mcp/info", () => Results.Json(new
{
    name = "AI Calendar MCP Server",
    version = "1.0.0",
    description = "MCP Server for AI Calendar WebAPI integration",
    capabilities = new
    {
        resources = true,
        tools = true
    }
}));

app.Run("http://localhost:5118");
