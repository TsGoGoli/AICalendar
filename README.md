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

1. **Configure VS Code:**
   - Open VS Code in the project directory
   - The `.vscode/settings.json` is already configured

2. **Use with GitHub Copilot:**
   - Open GitHub Copilot Chat in VS Code
   - Type `@aicalendar` to interact with the calendar MCP server
   - Example queries:
     - `@aicalendar Can you show statistics for the previous week?`
     - `@aicalendar Create a new meeting for tomorrow at 2 PM`
     - `@aicalendar Find available time slots for next week`

#### Available MCP Features:
- **Resources:** Get events, event participants, calendar statistics
- **Tools:** Create events, get statistics for date ranges
- **Analytics:** Meeting duration analysis, daily breakdowns

### Testing the MCP Server

Run the test script to verify MCP server functionality:
```bash
./test-mcp.ps1
```