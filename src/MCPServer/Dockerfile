# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY AICalendar.sln ./
COPY src/Domain/AICalendar.Domain.csproj src/Domain/
COPY src/Application/AICalendar.Application.csproj src/Application/
COPY src/Infrastructure/AICalendar.Infrastructure.csproj src/Infrastructure/
COPY src/WebAPI/AICalendar.WebAPI.csproj src/WebAPI/
COPY src/MCPServer/AICalendar.MCPServer.csproj src/MCPServer/
RUN dotnet restore src/MCPServer/AICalendar.MCPServer.csproj

# Copy source and publish
COPY . .
RUN dotnet publish src/MCPServer/AICalendar.MCPServer.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# No ports; MCP server communicates over stdin/stdout
ENTRYPOINT ["dotnet", "AICalendar.MCPServer.dll"]
