using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Enums;

namespace AICalendar.MCPServer;

/// <summary>
/// Main MCP Server implementation for AI Calendar
/// </summary>
public class CalendarMCPServer
{
    private readonly CalendarApiClient _apiClient;
    private readonly ILogger<CalendarMCPServer> _logger;
    private readonly IConfiguration _configuration;

    public CalendarMCPServer(CalendarApiClient apiClient, ILogger<CalendarMCPServer> logger, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get server information
    /// </summary>
    public object GetServerInfo()
    {
        return new
        {
            Name = _configuration.GetValue<string>("MCPServer:Name") ?? "AICalendar",
            Version = _configuration.GetValue<string>("MCPServer:Version") ?? "1.0.0",
            Description = _configuration.GetValue<string>("MCPServer:Description") ?? "AI Calendar MCP Server"
        };
    }

    /// <summary>
    /// List available resources
    /// </summary>
    public object[] ListResources()
    {
        return new object[]
        {
            new
            {
                Uri = "calendar://events",
                Name = "Events List",
                Description = "Get all calendar events with optional filtering",
                MimeType = "application/json"
            },
            new
            {
                Uri = "calendar://events/{eventId}",
                Name = "Event Details",
                Description = "Get detailed information about a specific event",
                MimeType = "application/json"
            },
            new
            {
                Uri = "calendar://events/{eventId}/participants",
                Name = "Event Participants",
                Description = "Get all participants for a specific event",
                MimeType = "application/json"
            }
        };
    }

    /// <summary>
    /// Handle resource requests
    /// </summary>
    public async Task<object> GetResourceAsync(string uri, Dictionary<string, object>? arguments = null)
    {
        _logger.LogInformation("Getting resource: {Uri} with arguments: {Arguments}", uri, JsonSerializer.Serialize(arguments));

        try
        {
            return uri switch
            {
                "calendar://events" => await GetEventsResourceAsync(arguments),
                var eventUri when eventUri.StartsWith("calendar://events/") && eventUri.EndsWith("/participants") =>
                    await GetEventParticipantsResourceAsync(ExtractEventIdFromUri(eventUri)),
                var eventUri when eventUri.StartsWith("calendar://events/") =>
                    await GetEventResourceAsync(ExtractEventIdFromUri(eventUri)),
                _ => throw new ArgumentException($"Unknown resource: {uri}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource {Uri}", uri);
            throw;
        }
    }

    /// <summary>
    /// List available tools
    /// </summary>
    public object[] ListTools()
    {
        return new object[]
        {
            new
            {
                Name = "create_event",
                Description = "Create a new calendar event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        title = new { type = "string", description = "Event title" },
                        description = new { type = "string", description = "Event description" },
                        start = new { type = "string", format = "date-time", description = "Event start time" },
                        end = new { type = "string", format = "date-time", description = "Event end time" },
                        organizerId = new { type = "string", format = "uuid", description = "Organizer user ID" }
                    },
                    Required = new[] { "title", "start", "end", "organizerId" }
                }
            },
            new
            {
                Name = "update_event",
                Description = "Update an existing calendar event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        eventId = new { type = "string", format = "uuid", description = "Event ID to update" },
                        title = new { type = "string", description = "Event title" },
                        description = new { type = "string", description = "Event description" },
                        start = new { type = "string", format = "date-time", description = "Event start time" },
                        end = new { type = "string", format = "date-time", description = "Event end time" }
                    },
                    Required = new[] { "eventId" }
                }
            },
            new
            {
                Name = "delete_event",
                Description = "Delete a calendar event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        eventId = new { type = "string", format = "uuid", description = "Event ID to delete" }
                    },
                    Required = new[] { "eventId" }
                }
            },
            new
            {
                Name = "add_participant",
                Description = "Add a participant to an event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        eventId = new { type = "string", format = "uuid", description = "Event ID" },
                        userId = new { type = "string", format = "uuid", description = "User ID to add" },
                        status = new { type = "integer", description = "Participant status (0=Pending, 1=Accepted, 2=Declined)" }
                    },
                    Required = new[] { "eventId", "userId" }
                }
            },
            new
            {
                Name = "update_participant_status",
                Description = "Update participant status for an event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        eventId = new { type = "string", format = "uuid", description = "Event ID" },
                        userId = new { type = "string", format = "uuid", description = "User ID" },
                        status = new { type = "integer", description = "New status (0=Pending, 1=Accepted, 2=Declined)" }
                    },
                    Required = new[] { "eventId", "userId", "status" }
                }
            },
            new
            {
                Name = "remove_participant",
                Description = "Remove a participant from an event",
                InputSchema = new
                {
                    Type = "object",
                    Properties = new
                    {
                        eventId = new { type = "string", format = "uuid", description = "Event ID" },
                        userId = new { type = "string", format = "uuid", description = "User ID to remove" }
                    },
                    Required = new[] { "eventId", "userId" }
                }
            }
        };
    }

    /// <summary>
    /// Execute a tool
    /// </summary>
    public async Task<object> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        _logger.LogInformation("Executing tool: {ToolName} with arguments: {Arguments}", toolName, JsonSerializer.Serialize(arguments));

        try
        {
            return toolName switch
            {
                "create_event" => await CreateEventToolAsync(arguments),
                "update_event" => await UpdateEventToolAsync(arguments),
                "delete_event" => await DeleteEventToolAsync(arguments),
                "add_participant" => await AddParticipantToolAsync(arguments),
                "update_participant_status" => await UpdateParticipantStatusToolAsync(arguments),
                "remove_participant" => await RemoveParticipantToolAsync(arguments),
                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
            throw;
        }
    }

    /// <summary>
    /// List available prompts
    /// </summary>
    public object[] ListPrompts()
    {
        return new object[]
        {
            new
            {
                Name = "schedule_meeting",
                Description = "Schedule a meeting with multiple participants",
                Arguments = new object[]
                {
                    new { name = "title", description = "Meeting title", required = true },
                    new { name = "description", description = "Meeting description", required = false },
                    new { name = "duration", description = "Meeting duration in minutes", required = true },
                    new { name = "participants", description = "Comma-separated list of participant user IDs", required = true },
                    new { name = "organizerId", description = "Organizer user ID", required = true }
                }
            },
            new
            {
                Name = "find_best_time",
                Description = "Find the best available time slot for a group meeting",
                Arguments = new object[]
                {
                    new { name = "participants", description = "Comma-separated list of participant user IDs", required = true },
                    new { name = "duration", description = "Meeting duration in minutes", required = true },
                    new { name = "startDate", description = "Search start date (YYYY-MM-DD)", required = false },
                    new { name = "endDate", description = "Search end date (YYYY-MM-DD)", required = false }
                }
            }
        };
    }

    // Private helper methods
    private Guid ExtractEventIdFromUri(string uri)
    {
        var parts = uri.Split('/');
        var eventIdStr = parts.Length > 2 ? parts[2] : throw new ArgumentException("Invalid URI format");
        return Guid.Parse(eventIdStr);
    }

    private async Task<object> GetEventsResourceAsync(Dictionary<string, object>? arguments)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;
        int? status = null;

        if (arguments != null)
        {
            if (arguments.TryGetValue("startDate", out var start) && start is string startStr)
                DateTime.TryParse(startStr, out var parsedStart);
            
            if (arguments.TryGetValue("endDate", out var end) && end is string endStr)
                DateTime.TryParse(endStr, out var parsedEnd);
            
            if (arguments.TryGetValue("status", out var statusObj) && statusObj is int statusInt)
                status = statusInt;
        }

        var events = await _apiClient.GetEventsAsync(startDate, endDate, status);
        return new { events = events ?? new List<EventDto>() };
    }

    private async Task<object> GetEventResourceAsync(Guid eventId)
    {
        var eventDto = await _apiClient.GetEventByIdAsync(eventId);
        return new { @event = eventDto };
    }

    private async Task<object> GetEventParticipantsResourceAsync(Guid eventId)
    {
        var participants = await _apiClient.GetEventParticipantsAsync(eventId);
        return new { participants = participants ?? new List<ParticipantDto>() };
    }

    private async Task<object> CreateEventToolAsync(Dictionary<string, object> arguments)
    {
        var createDto = new CreateEventDto
        {
            Title = arguments["title"].ToString() ?? "",
            Description = arguments.TryGetValue("description", out var desc) ? desc?.ToString() : null,
            Start = DateTime.Parse(arguments["start"].ToString() ?? ""),
            End = DateTime.Parse(arguments["end"].ToString() ?? "")
        };

        var organizerId = Guid.Parse(arguments["organizerId"].ToString() ?? "");
        var result = await _apiClient.CreateEventAsync(createDto, organizerId);
        
        return new { success = result != null, @event = result };
    }

    private async Task<object> UpdateEventToolAsync(Dictionary<string, object> arguments)
    {
        var eventId = Guid.Parse(arguments["eventId"].ToString() ?? "");
        var updateDto = new UpdateEventDto();

        if (arguments.TryGetValue("title", out var title))
            updateDto.Title = title.ToString() ?? "";
        
        if (arguments.TryGetValue("description", out var desc))
            updateDto.Description = desc?.ToString();
        
        if (arguments.TryGetValue("start", out var start))
            updateDto.Start = DateTime.Parse(start.ToString() ?? "");
        
        if (arguments.TryGetValue("end", out var end))
            updateDto.End = DateTime.Parse(end.ToString() ?? "");

        var success = await _apiClient.UpdateEventAsync(eventId, updateDto);
        return new { success };
    }

    private async Task<object> DeleteEventToolAsync(Dictionary<string, object> arguments)
    {
        var eventId = Guid.Parse(arguments["eventId"].ToString() ?? "");
        var success = await _apiClient.DeleteEventAsync(eventId);
        return new { success };
    }

    private async Task<object> AddParticipantToolAsync(Dictionary<string, object> arguments)
    {
        var eventId = Guid.Parse(arguments["eventId"].ToString() ?? "");
        var userId = Guid.Parse(arguments["userId"].ToString() ?? "");
        var status = arguments.TryGetValue("status", out var statusObj) ? 
            (ParticipantStatus)(int)statusObj : ParticipantStatus.Pending;

        var addDto = new AddParticipantDto
        {
            UserId = userId,
            Status = status
        };

        var result = await _apiClient.AddParticipantAsync(eventId, addDto);
        return new { success = result != null, participant = result };
    }

    private async Task<object> UpdateParticipantStatusToolAsync(Dictionary<string, object> arguments)
    {
        var eventId = Guid.Parse(arguments["eventId"].ToString() ?? "");
        var userId = Guid.Parse(arguments["userId"].ToString() ?? "");
        var status = (ParticipantStatus)(int)arguments["status"];

        var updateDto = new UpdateParticipantStatusDto
        {
            Status = status
        };

        var success = await _apiClient.UpdateParticipantStatusAsync(eventId, userId, updateDto);
        return new { success };
    }

    private async Task<object> RemoveParticipantToolAsync(Dictionary<string, object> arguments)
    {
        var eventId = Guid.Parse(arguments["eventId"].ToString() ?? "");
        var userId = Guid.Parse(arguments["userId"].ToString() ?? "");

        var success = await _apiClient.RemoveParticipantAsync(eventId, userId);
        return new { success };
    }
}
