# AI Calendar WebAPI

A modern calendar application with AI integration that allows users to manage their personal calendars and find available time slots among participants.

## Features

- Multi-user calendar management
- Event creation, updating, and deletion
- Participant management for events
- Smart scheduling with available slot detection
- AI integration via MCP Server
- Clean Architecture and DDD principles
- Built with .NET 9 and Aspire

## Technology Stack

- .NET 9
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQL Server
- CQRS with MediatR
- FluentValidation
- Serilog
- Docker
- Microsoft AI MCP SDK

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Visual Studio Code or Visual Studio 2022

### Running the Application

1. **Start the Web API:**
   ```bash
   dotnet run --project src/WebAPI/AICalendar.WebAPI.csproj
   ```

2. **Start the MCP Server:**
   ```bash
   dotnet run --project src/MCPServer/AICalendar.MCPServer.csproj
   ```

### GitHub Copilot Integration

This project includes an MCP (Model Context Protocol) Server that integrates with GitHub Copilot.

#### Setup Steps:

1. **Start the MCP Server:**
   ```bash
   cd src/MCPServer
   dotnet run
   ```
   The server will start on `http://localhost:5119`

2. **Configure VS Code:**
   - The `.vscode/settings.json` is already configured
   - The `mcp-config.json` contains the server configuration

3. **Use with GitHub Copilot:**
   - Open GitHub Copilot Chat in VS Code
   - Type `@aicalendar` to interact with the calendar MCP server
   - Example queries:
     - `@aicalendar Can you show statistics for the previous week?`
     - `@aicalendar Create a new meeting for tomorrow at 2 PM`
     - `@aicalendar Find available time slots for next week`

#### Available MCP Features:
- **Resources:** 
  - `calendar://events` - Get all events
  - `calendar://events/{eventId}/participants` - Get event participants
  - `calendar://statistics/{startDate}/{endDate}` - Get calendar statistics
- **Tools:** 
  - `create_event` - Create new calendar events
  - `get_statistics` - Get statistics for date ranges
- **Analytics:** Meeting duration analysis, daily breakdowns, participant insights

#### Testing the MCP Server

1. **Health Check:**
   ```bash
   curl http://localhost:5119/health
   ```

2. **MCP Info:**
   ```bash
   curl http://localhost:5119/mcp/info
   ```

3. **Run Test Script:**
   ```bash
   ./test-mcp.ps1
   ```