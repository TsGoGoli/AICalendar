using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using AICalendar.Application.DTOs;

namespace AICalendar.MCPServer;

/// <summary>
/// HTTP client for communicating with the AI Calendar Web API
/// </summary>
public class CalendarApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalendarApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _authToken;
    private DateTime? _tokenExpiry;

    public CalendarApiClient(HttpClient httpClient, ILogger<CalendarApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }    // Authentication Methods
    public async Task<bool> LoginAsync(string email = "admin@aicalendar.com", string password = "Admin123!")
    {
        try
        {
            var loginDto = new LoginDto
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, _jsonOptions);
                
                if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.Token))
                {
                    _authToken = authResponse.Token;
                    _tokenExpiry = authResponse.ExpiresAt;
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                    
                    _logger.LogInformation("Successfully authenticated user {Email}", email);
                    return true;
                }
            }

            _logger.LogWarning("Failed to authenticate. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return false;
        }
    }

    private async Task<bool> EnsureAuthenticatedAsync()
    {
        // Check if token is still valid (with 5 minutes buffer)
        if (!string.IsNullOrEmpty(_authToken) && 
            _tokenExpiry.HasValue && 
            _tokenExpiry.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return true;
        }

        // Re-authenticate
        return await LoginAsync();
    }    // Event Resources
    public async Task<IReadOnlyList<EventDto>?> GetEventsAsync(DateTime? startDate = null, DateTime? endDate = null, int? status = null)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                _logger.LogWarning("Failed to authenticate for GetEvents");
                return null;
            }

            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}");
            if (status.HasValue) queryParams.Add($"status={status.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/events{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IReadOnlyList<EventDto>>(content, _jsonOptions);
            }

            _logger.LogWarning("Failed to get events. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events");
            throw;
        }
    }    public async Task<EventDto?> GetEventByIdAsync(Guid eventId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                _logger.LogWarning("Failed to authenticate for GetEventById");
                return null;
            }

            var response = await _httpClient.GetAsync($"/api/events/{eventId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EventDto>(content, _jsonOptions);
            }

            _logger.LogWarning("Failed to get event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event {EventId}", eventId);
            throw;
        }
    }

    public async Task<IReadOnlyList<ParticipantDto>?> GetEventParticipantsAsync(Guid eventId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                _logger.LogWarning("Failed to authenticate for GetEventParticipants");
                return null;
            }

            var response = await _httpClient.GetAsync($"/api/events/{eventId}/participants");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IReadOnlyList<ParticipantDto>>(content, _jsonOptions);
            }

            _logger.LogWarning("Failed to get participants for event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for event {EventId}", eventId);
            throw;
        }
    }    // Event Tools
    public async Task<EventDto?> CreateEventAsync(CreateEventDto eventDto, Guid organizerId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                _logger.LogWarning("Failed to authenticate for CreateEvent");
                return null;
            }

            var json = JsonSerializer.Serialize(eventDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/api/events?organizerId={organizerId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EventDto>(responseContent, _jsonOptions);
            }

            _logger.LogWarning("Failed to create event. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    public async Task<bool> UpdateEventAsync(Guid eventId, UpdateEventDto eventDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(eventDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/events/{eventId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning("Failed to update event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> DeleteEventAsync(Guid eventId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/events/{eventId}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning("Failed to delete event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", eventId);
            throw;
        }
    }

    // Participant Tools
    public async Task<ParticipantDto?> AddParticipantAsync(Guid eventId, AddParticipantDto participantDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(participantDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/api/events/{eventId}/participants", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ParticipantDto>(responseContent, _jsonOptions);
            }

            _logger.LogWarning("Failed to add participant to event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to event {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> UpdateParticipantStatusAsync(Guid eventId, Guid userId, UpdateParticipantStatusDto statusDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(statusDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/events/{eventId}/participants/{userId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning("Failed to update participant status for event {EventId}, user {UserId}. Status: {StatusCode}", eventId, userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant status for event {EventId}, user {UserId}", eventId, userId);
            throw;
        }
    }

    public async Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/events/{eventId}/participants/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning("Failed to remove participant from event {EventId}, user {UserId}. Status: {StatusCode}", eventId, userId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from event {EventId}, user {UserId}", eventId, userId);
            throw;
        }
    }
}
