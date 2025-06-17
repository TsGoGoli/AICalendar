using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AICalendar.MCPServer;

/// <summary>
/// Handles MCP protocol communication via stdin/stdout
/// </summary>
public class MCPHandler
{
    private readonly CalendarMCPServer _calendarServer;
    private readonly ILogger<MCPHandler> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MCPHandler(CalendarMCPServer calendarServer, ILogger<MCPHandler> logger)
    {
        _calendarServer = calendarServer;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Handler started, listening for messages...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    _logger.LogInformation("End of input stream reached");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    await ProcessMessageAsync(line);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", line);
                    await SendErrorResponse(null, -32603, "Internal error", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in MCP handler");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string messageJson)
    {
        MCPProtocol.Message? message;
        
        try
        {
            message = JsonSerializer.Deserialize<MCPProtocol.Message>(messageJson, _jsonOptions);
            if (message == null)
            {
                await SendErrorResponse(null, -32700, "Parse error", "Invalid JSON");
                return;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON message: {Message}", messageJson);
            await SendErrorResponse(null, -32700, "Parse error", ex.Message);
            return;
        }

        _logger.LogDebug("Processing method: {Method}", message.method);

        try
        {
            switch (message.method)
            {
                case "initialize":
                    await HandleInitialize(message);
                    break;
                
                case "initialized":
                    // Notification - no response needed
                    _logger.LogInformation("Client initialized");
                    break;

                case "resources/list":
                    await HandleListResources(message);
                    break;

                case "resources/read":
                    await HandleReadResource(message);
                    break;

                case "tools/list":
                    await HandleListTools(message);
                    break;

                case "tools/call":
                    await HandleCallTool(message);
                    break;

                case "prompts/list":
                    await HandleListPrompts(message);
                    break;

                default:
                    await SendErrorResponse(message.id, -32601, "Method not found", $"Unknown method: {message.method}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling method {Method}", message.method);
            await SendErrorResponse(message.id, -32603, "Internal error", ex.Message);
        }
    }

    private async Task HandleInitialize(MCPProtocol.Message message)
    {
        var serverInfo = _calendarServer.GetServerInfo();
        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.InitializeResult
            {
                protocolVersion = "2024-11-05",
                capabilities = new MCPProtocol.ServerCapabilities
                {
                    logging = true,
                    prompts = new MCPProtocol.PromptCapability { listChanged = true },
                    resources = new MCPProtocol.ResourceCapability { subscribe = true, listChanged = true },
                    tools = new MCPProtocol.ToolCapability { listChanged = true }
                },
                serverInfo = new MCPProtocol.ServerInfo
                {
                    name = serverInfo.GetType().GetProperty("Name")?.GetValue(serverInfo)?.ToString() ?? "AICalendar",
                    version = serverInfo.GetType().GetProperty("Version")?.GetValue(serverInfo)?.ToString() ?? "1.0.0"
                }
            }
        };

        await SendMessage(response);
    }

    private async Task HandleListResources(MCPProtocol.Message message)
    {
        var resources = _calendarServer.ListResources();
        var mcpResources = resources.Select(r => new MCPProtocol.Resource
        {
            uri = r.GetType().GetProperty("Uri")?.GetValue(r)?.ToString() ?? "",
            name = r.GetType().GetProperty("Name")?.GetValue(r)?.ToString() ?? "",
            description = r.GetType().GetProperty("Description")?.GetValue(r)?.ToString(),
            mimeType = r.GetType().GetProperty("MimeType")?.GetValue(r)?.ToString()
        }).ToArray();

        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.ListResourcesResult
            {
                resources = mcpResources
            }
        };

        await SendMessage(response);
    }

    private async Task HandleReadResource(MCPProtocol.Message message)
    {
        var paramsElement = (JsonElement)message.@params!;
        var uri = paramsElement.GetProperty("uri").GetString()!;

        var result = await _calendarServer.GetResourceAsync(uri);
        var content = JsonSerializer.Serialize(result, _jsonOptions);

        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.ReadResourceResult
            {
                contents = new[]
                {
                    new MCPProtocol.ResourceContent
                    {
                        uri = uri,
                        mimeType = "application/json",
                        text = content
                    }
                }
            }
        };

        await SendMessage(response);
    }

    private async Task HandleListTools(MCPProtocol.Message message)
    {
        var tools = _calendarServer.ListTools();
        var mcpTools = tools.Select(t => new MCPProtocol.Tool
        {
            name = t.GetType().GetProperty("Name")?.GetValue(t)?.ToString() ?? "",
            description = t.GetType().GetProperty("Description")?.GetValue(t)?.ToString(),
            inputSchema = t.GetType().GetProperty("InputSchema")?.GetValue(t)
        }).ToArray();

        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.ListToolsResult
            {
                tools = mcpTools
            }
        };

        await SendMessage(response);
    }

    private async Task HandleCallTool(MCPProtocol.Message message)
    {
        var paramsElement = (JsonElement)message.@params!;
        var toolName = paramsElement.GetProperty("name").GetString()!;
        var argumentsElement = paramsElement.TryGetProperty("arguments", out var args) ? args : (JsonElement?)null;

        var arguments = new Dictionary<string, object>();
        if (argumentsElement.HasValue)
        {
            foreach (var prop in argumentsElement.Value.EnumerateObject())
            {
                arguments[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString()!,
                    JsonValueKind.Number => prop.Value.GetInt32(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => prop.Value.ToString()
                };
            }
        }

        var result = await _calendarServer.ExecuteToolAsync(toolName, arguments);
        var content = JsonSerializer.Serialize(result, _jsonOptions);

        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.CallToolResult
            {
                content = new[]
                {
                    new MCPProtocol.ToolResult
                    {
                        type = "text",
                        text = content
                    }
                },
                isError = false
            }
        };

        await SendMessage(response);
    }

    private async Task HandleListPrompts(MCPProtocol.Message message)
    {
        var prompts = _calendarServer.ListPrompts();
        var mcpPrompts = prompts.Select(p => new MCPProtocol.Prompt
        {
            name = p.GetType().GetProperty("Name")?.GetValue(p)?.ToString() ?? "",
            description = p.GetType().GetProperty("Description")?.GetValue(p)?.ToString(),
            arguments = ((object[]?)p.GetType().GetProperty("Arguments")?.GetValue(p))?.Select(arg => new MCPProtocol.PromptArgument
            {
                name = arg.GetType().GetProperty("name")?.GetValue(arg)?.ToString() ?? "",
                description = arg.GetType().GetProperty("description")?.GetValue(arg)?.ToString(),
                required = (bool)(arg.GetType().GetProperty("required")?.GetValue(arg) ?? false)
            }).ToArray()
        }).ToArray();

        var response = new MCPProtocol.Message
        {
            id = message.id,
            result = new MCPProtocol.ListPromptsResult
            {
                prompts = mcpPrompts
            }
        };

        await SendMessage(response);
    }

    private async Task SendMessage(MCPProtocol.Message message)
    {
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        await Console.Out.WriteLineAsync(json);
        await Console.Out.FlushAsync();
        _logger.LogDebug("Sent response: {Response}", json);
    }

    private async Task SendErrorResponse(object? id, int code, string message, string? data = null)
    {
        var response = new MCPProtocol.Message
        {
            id = id,
            error = new
            {
                code,
                message,
                data
            }
        };

        await SendMessage(response);
    }
}
