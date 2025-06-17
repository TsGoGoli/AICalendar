using System.Text.Json;

namespace AICalendar.MCPServer;

/// <summary>
/// MCP (Model Context Protocol) message types and structures
/// </summary>
public static class MCPProtocol
{
    public class Message
    {
        public string jsonrpc { get; set; } = "2.0";
        public object? id { get; set; }
        public string? method { get; set; }
        public object? @params { get; set; }
        public object? result { get; set; }
        public object? error { get; set; }
    }

    public class InitializeParams
    {
        public string protocolVersion { get; set; } = "2024-11-05";
        public ClientCapabilities capabilities { get; set; } = new();
        public ClientInfo clientInfo { get; set; } = new();
    }

    public class ClientCapabilities
    {
        public RootsCapability? roots { get; set; }
        public SamplingCapability? sampling { get; set; }
    }

    public class RootsCapability
    {
        public bool listChanged { get; set; }
    }

    public class SamplingCapability
    {
    }

    public class ClientInfo
    {
        public string name { get; set; } = "";
        public string version { get; set; } = "";
    }

    public class InitializeResult
    {
        public string protocolVersion { get; set; } = "2024-11-05";
        public ServerCapabilities capabilities { get; set; } = new();
        public ServerInfo serverInfo { get; set; } = new();
    }

    public class ServerCapabilities
    {
        public bool logging { get; set; } = true;
        public PromptCapability? prompts { get; set; } = new() { listChanged = true };
        public ResourceCapability? resources { get; set; } = new() { subscribe = true, listChanged = true };
        public ToolCapability? tools { get; set; } = new() { listChanged = true };
    }

    public class PromptCapability
    {
        public bool listChanged { get; set; }
    }

    public class ResourceCapability
    {
        public bool subscribe { get; set; }
        public bool listChanged { get; set; }
    }

    public class ToolCapability
    {
        public bool listChanged { get; set; }
    }

    public class ServerInfo
    {
        public string name { get; set; } = "";
        public string version { get; set; } = "";
    }

    public class ListResourcesResult
    {
        public Resource[] resources { get; set; } = Array.Empty<Resource>();
    }

    public class Resource
    {
        public string uri { get; set; } = "";
        public string name { get; set; } = "";
        public string? description { get; set; }
        public string? mimeType { get; set; }
    }

    public class ReadResourceParams
    {
        public string uri { get; set; } = "";
    }

    public class ReadResourceResult
    {
        public ResourceContent[] contents { get; set; } = Array.Empty<ResourceContent>();
    }

    public class ResourceContent
    {
        public string uri { get; set; } = "";
        public string mimeType { get; set; } = "";
        public object? text { get; set; }
        public object? blob { get; set; }
    }

    public class ListToolsResult
    {
        public Tool[] tools { get; set; } = Array.Empty<Tool>();
    }

    public class Tool
    {
        public string name { get; set; } = "";
        public string? description { get; set; }
        public object? inputSchema { get; set; }
    }

    public class CallToolParams
    {
        public string name { get; set; } = "";
        public object? arguments { get; set; }
    }

    public class CallToolResult
    {
        public ToolResult[] content { get; set; } = Array.Empty<ToolResult>();
        public bool isError { get; set; } = false;
    }

    public class ToolResult
    {
        public string type { get; set; } = "text";
        public string text { get; set; } = "";
    }

    public class ListPromptsResult
    {
        public Prompt[] prompts { get; set; } = Array.Empty<Prompt>();
    }

    public class Prompt
    {
        public string name { get; set; } = "";
        public string? description { get; set; }
        public PromptArgument[]? arguments { get; set; }
    }

    public class PromptArgument
    {
        public string name { get; set; } = "";
        public string? description { get; set; }
        public bool required { get; set; } = false;
    }
}
