using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AICalendar.MCPServer.Services;

public class CalendarApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalendarApiClient> _logger;
    private readonly string _baseUrl;

    public CalendarApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<CalendarApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<string> GetEventsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/events");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events from API");
            throw;
        }
    }

    public async Task<string> GetEventParticipantsAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/events/{eventId}/participants");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event participants from API for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<string> CreateEventAsync(string jsonPayload)
    {
        try
        {
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/events", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event via API");
            throw;
        }
    }

    public async Task<string> GetEventStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/events?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
            response.EnsureSuccessStatusCode();
            var eventsJson = await response.Content.ReadAsStringAsync();
            
            // Parse and create statistics
            var events = JsonSerializer.Deserialize<JsonElement>(eventsJson);
            return CreateStatisticsReport(events, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event statistics from API");
            throw;
        }
    }

    private string CreateStatisticsReport(JsonElement events, DateTime startDate, DateTime endDate)
    {
        // This method creates a statistics report similar to what's shown in the screenshot
        var eventList = events.GetProperty("value").EnumerateArray().ToList();
        var totalEvents = eventList.Count;
        
        var report = new
        {
            period = $"{startDate:MMM d} - {endDate:MMM d, yyyy}",
            totalEvents = totalEvents,
            totalDuration = CalculateTotalDuration(eventList),
            averageDuration = CalculateAverageDuration(eventList),
            dailyBreakdown = CreateDailyBreakdown(eventList, startDate, endDate),
            durationAnalysis = CreateDurationAnalysis(eventList)
        };

        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    private string CalculateTotalDuration(List<JsonElement> events)
    {
        var totalMinutes = 0;
        foreach (var evt in events)
        {
            if (evt.TryGetProperty("startDate", out var start) && 
                evt.TryGetProperty("endDate", out var end))
            {
                if (DateTime.TryParse(start.GetString(), out var startTime) &&
                    DateTime.TryParse(end.GetString(), out var endTime))
                {
                    totalMinutes += (int)(endTime - startTime).TotalMinutes;
                }
            }
        }
        
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours} hours {minutes} minutes ({totalMinutes} minutes)";
    }

    private string CalculateAverageDuration(List<JsonElement> events)
    {
        if (events.Count == 0) return "0 minutes";
        
        var totalMinutes = 0;
        var validEvents = 0;
        
        foreach (var evt in events)
        {
            if (evt.TryGetProperty("startDate", out var start) && 
                evt.TryGetProperty("endDate", out var end))
            {
                if (DateTime.TryParse(start.GetString(), out var startTime) &&
                    DateTime.TryParse(end.GetString(), out var endTime))
                {
                    totalMinutes += (int)(endTime - startTime).TotalMinutes;
                    validEvents++;
                }
            }
        }
        
        if (validEvents == 0) return "0 minutes";
        
        var avgMinutes = totalMinutes / validEvents;
        return $"{avgMinutes} minutes";
    }

    private object CreateDailyBreakdown(List<JsonElement> events, DateTime startDate, DateTime endDate)
    {
        var dailyStats = new List<object>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayEvents = events.Where(evt => 
            {
                if (evt.TryGetProperty("startDate", out var start) &&
                    DateTime.TryParse(start.GetString(), out var startTime))
                {
                    return startTime.Date == date.Date;
                }
                return false;
            }).ToList();

            var totalDuration = 0;
            foreach (var evt in dayEvents)
            {
                if (evt.TryGetProperty("startDate", out var start) && 
                    evt.TryGetProperty("endDate", out var end))
                {
                    if (DateTime.TryParse(start.GetString(), out var startTime) &&
                        DateTime.TryParse(end.GetString(), out var endTime))
                    {
                        totalDuration += (int)(endTime - startTime).TotalMinutes;
                    }
                }
            }

            dailyStats.Add(new
            {
                day = date.DayOfWeek.ToString(),
                date = date.ToString("MMM d"),
                events = dayEvents.Count,
                totalDuration = totalDuration > 0 ? $"{totalDuration / 60}h {totalDuration % 60}m" : "0m"
            });
        }
        
        return dailyStats;
    }

    private object CreateDurationAnalysis(List<JsonElement> events)
    {
        var shortMeetings = 0;
        var mediumMeetings = 0;
        var longMeetings = 0;

        foreach (var evt in events)
        {
            if (evt.TryGetProperty("startDate", out var start) && 
                evt.TryGetProperty("endDate", out var end))
            {
                if (DateTime.TryParse(start.GetString(), out var startTime) &&
                    DateTime.TryParse(end.GetString(), out var endTime))
                {
                    var duration = (int)(endTime - startTime).TotalMinutes;
                    if (duration <= 30) shortMeetings++;
                    else if (duration <= 90) mediumMeetings++;
                    else longMeetings++;
                }
            }
        }

        return new
        {
            shortMeetings = $"{shortMeetings} events ({(events.Count > 0 ? (shortMeetings * 100 / events.Count) : 0)}%)",
            mediumMeetings = $"{mediumMeetings} events ({(events.Count > 0 ? (mediumMeetings * 100 / events.Count) : 0)}%)",
            longMeetings = $"{longMeetings} events ({(events.Count > 0 ? (longMeetings * 100 / events.Count) : 0)}%)"
        };
    }
}
