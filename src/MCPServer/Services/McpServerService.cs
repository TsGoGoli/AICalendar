using AICalendar.MCPServer.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AICalendar.MCPServer.Services;

public class McpServerService
{
    private readonly CalendarApiClient _apiClient;
    private readonly ILogger<McpServerService> _logger;

    public McpServerService(CalendarApiClient apiClient, ILogger<McpServerService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        _logger.LogInformation("Handling MCP request: {Method}", request.Method);

        try
        {
            return request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "resources/list" => await HandleListResourcesAsync(request),
                "resources/read" => await HandleReadResourceAsync(request),
                "tools/list" => await HandleListToolsAsync(request),
                "tools/call" => await HandleToolCallAsync(request),
                _ => CreateErrorResponse(request.Id, -32601, "Method not found")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP request");
            return CreateErrorResponse(request.Id, -32603, "Internal error");
        }
    }

    private McpResponse HandleInitialize(McpRequest request)
    {
        return new McpResponse
        {
            Id = request.Id,
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    resources = new { },
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "AI Calendar MCP Server",
                    version = "1.0.0"
                }
            }
        };
    }

    private Task<McpResponse> HandleListResourcesAsync(McpRequest request)
    {
        var resources = new[]
        {
            new McpResource
            {
                Uri = "calendar://events",
                Name = "events",
                Description = "List of all calendar events",
                MimeType = "application/json"
            },
            new McpResource
            {
                Uri = "calendar://events/{eventId}/participants",
                Name = "event-participants",
                Description = "Participants of a specific event",
                MimeType = "application/json"
            },
            new McpResource
            {
                Uri = "calendar://statistics/{startDate}/{endDate}",
                Name = "event-statistics",
                Description = "Statistics and analysis for events in a date range",
                MimeType = "application/json"
            }
        };

        return Task.FromResult(new McpResponse
        {
            Id = request.Id,
            Result = new { resources }
        });
    }

    private async Task<McpResponse> HandleReadResourceAsync(McpRequest request)
    {
        var uri = ExtractUriFromParams(request.Params);
        
        if (uri.StartsWith("calendar://statistics/"))
        {
            // Extract dates from URI like "calendar://statistics/2025-06-07/2025-06-13"
            var dateParts = uri.Replace("calendar://statistics/", "").Split('/');
            if (dateParts.Length == 2 && 
                DateTime.TryParse(dateParts[0], out var startDate) &&
                DateTime.TryParse(dateParts[1], out var endDate))
            {
                var statistics = await _apiClient.GetEventStatisticsAsync(startDate, endDate);
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new { contents = new[] { new { type = "text", text = statistics } } }
                };
            }
        }
        else if (uri.StartsWith("calendar://events"))
        {
            if (uri.Contains("/participants"))
            {
                var eventId = ExtractEventIdFromUri(uri);
                var participants = await _apiClient.GetEventParticipantsAsync(eventId);
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new { contents = new[] { new { type = "text", text = participants } } }
                };
            }
            else
            {
                var events = await _apiClient.GetEventsAsync();
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new { contents = new[] { new { type = "text", text = events } } }
                };
            }
        }

        return CreateErrorResponse(request.Id, -32602, "Invalid resource URI");
    }

    private Task<McpResponse> HandleListToolsAsync(McpRequest request)
    {
        var tools = new[]
        {
            new McpTool
            {
                Name = "create_event",
                Description = "Create a new calendar event",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        title = new { type = "string", description = "Event title" },
                        description = new { type = "string", description = "Event description" },
                        startDate = new { type = "string", format = "date-time", description = "Start date and time" },
                        endDate = new { type = "string", format = "date-time", description = "End date and time" },
                        participants = new { type = "array", items = new { type = "string" }, description = "Participant user IDs" }
                    },
                    required = new[] { "title", "startDate", "endDate" }
                }
            },
            new McpTool
            {
                Name = "get_statistics",
                Description = "Get calendar statistics for a date range",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        startDate = new { type = "string", format = "date", description = "Start date (YYYY-MM-DD)" },
                        endDate = new { type = "string", format = "date", description = "End date (YYYY-MM-DD)" }
                    },
                    required = new[] { "startDate", "endDate" }
                }
            }
        };

        return Task.FromResult(new McpResponse
        {
            Id = request.Id,
            Result = new { tools }
        });
    }

    private async Task<McpResponse> HandleToolCallAsync(McpRequest request)
    {
        var toolName = ExtractToolNameFromParams(request.Params);
        
        return toolName switch
        {
            "create_event" => await HandleCreateEventTool(request),
            "get_statistics" => await HandleGetStatisticsTool(request),
            _ => CreateErrorResponse(request.Id, -32602, "Unknown tool")
        };
    }

    private async Task<McpResponse> HandleCreateEventTool(McpRequest request)
    {
        var arguments = ExtractArgumentsFromParams(request.Params);
        var result = await _apiClient.CreateEventAsync(arguments);
        
        return new McpResponse
        {
            Id = request.Id,
            Result = new { content = new[] { new { type = "text", text = result } } }
        };
    }

    private async Task<McpResponse> HandleGetStatisticsTool(McpRequest request)
    {
        var args = ExtractArgumentsFromParams(request.Params);
        var argumentsJson = JsonSerializer.Deserialize<JsonElement>(args);
        
        if (argumentsJson.TryGetProperty("startDate", out var startDateProp) &&
            argumentsJson.TryGetProperty("endDate", out var endDateProp) &&
            DateTime.TryParse(startDateProp.GetString(), out var startDate) &&
            DateTime.TryParse(endDateProp.GetString(), out var endDate))
        {
            var statistics = await _apiClient.GetEventStatisticsAsync(startDate, endDate);
            return new McpResponse
            {
                Id = request.Id,
                Result = new { content = new[] { new { type = "text", text = statistics } } }
            };
        }

        return CreateErrorResponse(request.Id, -32602, "Invalid date parameters");
    }

    private static McpResponse CreateErrorResponse(string id, int code, string message)
    {
        return new McpResponse
        {
            Id = id,
            Error = new McpError { Code = code, Message = message }
        };
    }

    private static string ExtractUriFromParams(object? parameters)
    {
        if (parameters is JsonElement element && element.TryGetProperty("uri", out var uriProperty))
        {
            return uriProperty.GetString() ?? "";
        }
        return "";
    }

    private static string ExtractEventIdFromUri(string uri)
    {
        var parts = uri.Split('/');
        return parts.Length > 3 ? parts[3] : "";
    }

    private static string ExtractToolNameFromParams(object? parameters)
    {
        if (parameters is JsonElement element && element.TryGetProperty("name", out var nameProperty))
        {
            return nameProperty.GetString() ?? "";
        }
        return "";
    }

    private static string ExtractArgumentsFromParams(object? parameters)
    {
        if (parameters is JsonElement element && element.TryGetProperty("arguments", out var argsProperty))
        {
            return argsProperty.GetRawText();
        }
        return "{}";
    }
}
