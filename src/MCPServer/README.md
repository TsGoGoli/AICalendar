# AI Calendar MCP Server

This is a Model Context Protocol (MCP) server implementation for the AI Calendar application. It provides LLMs with the ability to interact with calendar events and participants through a standardized protocol.

## Features

### Resources (Read-only data)
- `calendar://events` - List all events with optional filtering
- `calendar://events/{eventId}` - Get specific event details
- `calendar://events/{eventId}/participants` - Get event participants

### Tools (Executable actions)
- `create_event` - Create a new calendar event
- `update_event` - Update an existing event
- `delete_event` - Delete an event
- `add_participant` - Add a participant to an event
- `update_participant_status` - Update participant status (Pending/Accepted/Declined)
- `remove_participant` - Remove a participant from an event

### Prompts (Workflow templates)
- `schedule_meeting` - Schedule a meeting with multiple participants
- `find_best_time` - Find optimal time slots for group meetings

## Configuration

The MCP server is configured through `appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001",
  "MCPServer": {
    "Name": "AICalendar",
    "Version": "1.0.0",
    "Description": "AI Calendar MCP Server for managing events and participants"
  }
}
```

## Usage

### Running the Server

```bash
dotnet run --project src/MCPServer
```

The server communicates via stdin/stdout using the MCP protocol (JSON-RPC 2.0).

### Integration with LLM Tools

The MCP server can be integrated with LLM tools that support the Model Context Protocol, such as:
- Claude Desktop
- Visual Studio Code with MCP extensions
- Custom LLM applications

### Example Tool Calls

#### Create Event
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "create_event",
    "arguments": {
      "title": "Team Standup",
      "description": "Daily team synchronization",
      "start": "2024-01-15T09:00:00Z",
      "end": "2024-01-15T09:30:00Z",
      "organizerId": "123e4567-e89b-12d3-a456-426614174000"
    }
  }
}
```

#### Add Participant
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "add_participant",
    "arguments": {
      "eventId": "123e4567-e89b-12d3-a456-426614174001",
      "userId": "123e4567-e89b-12d3-a456-426614174002",
      "status": 1
    }
  }
}
```

#### Get Events
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "resources/read",
  "params": {
    "uri": "calendar://events"
  }
}
```

## Architecture

The MCP server consists of several key components:

1. **MCPHandler** - Handles MCP protocol communication via stdin/stdout
2. **CalendarMCPServer** - Main server logic implementing resources, tools, and prompts
3. **CalendarApiClient** - HTTP client for communicating with the Web API
4. **MCPProtocol** - Data structures for MCP message types

## Dependencies

- **Microsoft.Extensions.Hosting** - For dependency injection and hosting
- **Microsoft.Extensions.Http** - For HTTP client services
- **System.Text.Json** - For JSON serialization
- **AICalendar.Application** - Application layer with DTOs and business logic
- **AICalendar.Domain** - Domain entities and enums

## Development

### Building
```bash
dotnet build src/MCPServer
```

### Testing
```bash
# Run the Python test script (requires Python 3)
python src/MCPServer/test_mcp.py

# Or manually test with JSON-RPC messages via stdin
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' | dotnet run --project src/MCPServer
```

## Error Handling

The server implements standard JSON-RPC 2.0 error codes:
- `-32700` Parse error
- `-32601` Method not found  
- `-32603` Internal error

All errors include descriptive messages and are logged for debugging.

## Security Considerations

- The MCP server requires the Web API to be running and accessible
- Authentication/authorization is handled by the underlying Web API
- Input validation is performed on all tool parameters
- Logging includes request/response details for audit trails
