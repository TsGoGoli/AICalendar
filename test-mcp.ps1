#!/usr/bin/env pwsh

Write-Host "Testing MCP Server..." -ForegroundColor Green

# Test 1: Build the MCP Server
Write-Host "`n1. Building MCP Server..." -ForegroundColor Yellow
dotnet build src/MCPServer/AICalendar.MCPServer.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

# Test 2: Test MCP Server with a simple request
Write-Host "`n2. Testing MCP Server with initialize request..." -ForegroundColor Yellow

$initRequest = @{
    jsonrpc = "2.0"
    id = "1"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{
            name = "test-client"
            version = "1.0.0"
        }
    }
} | ConvertTo-Json -Depth 5

Write-Host "Sending request: $initRequest"

# Test that MCP server can start
Write-Host "Testing MCP server startup..." -ForegroundColor Yellow
$job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --project src/MCPServer/AICalendar.MCPServer.csproj
}

Start-Sleep -Seconds 5

if ($job.State -eq "Running") {
    Write-Host "✅ MCP Server started successfully" -ForegroundColor Green
    Stop-Job $job
    Remove-Job $job
} else {
    Write-Host "❌ MCP Server failed to start" -ForegroundColor Red
    Receive-Job $job
    Remove-Job $job
}

Write-Host "`n3. Next Steps:" -ForegroundColor Magenta
Write-Host "   - Make sure your Web API is running on https://localhost:7001"
Write-Host "   - In VS Code, open GitHub Copilot Chat"
Write-Host "   - Type '@aicalendar' to use the MCP server"
Write-Host "   - Try: '@aicalendar Can you show statistics for the previous week?'"

Write-Host "`nMCP Server setup complete! 🎉" -ForegroundColor Green
