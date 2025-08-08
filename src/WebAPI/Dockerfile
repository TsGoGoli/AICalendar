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
RUN dotnet restore src/WebAPI/AICalendar.WebAPI.csproj

# Copy source and publish
COPY . .
RUN dotnet publish src/WebAPI/AICalendar.WebAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AICalendar.WebAPI.dll"]
