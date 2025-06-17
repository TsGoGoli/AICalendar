using AICalendar.MCPServer.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AICalendar.MCPServer.Services;

public class McpProtocolHandler
{
    private readonly McpServerService _mcpServerService;
    private readonly ILogger<McpProtocolHandler> _logger;

    public McpProtocolHandler(McpServerService mcpServerService, ILogger<McpProtocolHandler> logger)
    {
        _mcpServerService = mcpServerService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Protocol Handler starting...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    _logger.LogInformation("stdin closed, shutting down");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                _logger.LogDebug("Received request: {Request}", line);

                try
                {
                    var request = JsonSerializer.Deserialize<McpRequest>(line);
                    if (request != null)
                    {
                        var response = await _mcpServerService.HandleRequestAsync(request);
                        var responseJson = JsonSerializer.Serialize(response);
                        
                        _logger.LogDebug("Sending response: {Response}", responseJson);
                        await Console.Out.WriteLineAsync(responseJson);
                        await Console.Out.FlushAsync();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON request: {Request}", line);
                    
                    var errorResponse = new McpResponse
                    {
                        Id = "",
                        Error = new McpError 
                        { 
                            Code = -32700, 
                            Message = "Parse error" 
                        }
                    };
                    
                    var errorJson = JsonSerializer.Serialize(errorResponse);
                    await Console.Out.WriteLineAsync(errorJson);
                    await Console.Out.FlushAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing request: {Request}", line);
                    
                    var errorResponse = new McpResponse
                    {
                        Id = "",
                        Error = new McpError 
                        { 
                            Code = -32603, 
                            Message = "Internal error" 
                        }
                    };
                    
                    var errorJson = JsonSerializer.Serialize(errorResponse);
                    await Console.Out.WriteLineAsync(errorJson);
                    await Console.Out.FlushAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in MCP Protocol Handler");
            throw;
        }

        _logger.LogInformation("MCP Protocol Handler stopped");
    }
}
