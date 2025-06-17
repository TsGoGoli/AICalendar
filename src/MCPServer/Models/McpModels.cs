using System.Text.Json.Serialization;

namespace AICalendar.MCPServer.Models;

public record McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
    
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    
    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;
    
    [JsonPropertyName("params")]
    public object? Params { get; init; }
}

public record McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
    
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    
    [JsonPropertyName("result")]
    public object? Result { get; init; }
    
    [JsonPropertyName("error")]
    public McpError? Error { get; init; }
}

public record McpError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }
    
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; init; }
}

public record McpResource
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
    
    [JsonPropertyName("mimeType")]
    public string MimeType { get; init; } = "application/json";
}

public record McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
    
    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; init; } = new { };
}
