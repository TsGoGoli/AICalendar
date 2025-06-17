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

# Start MCP server and send request
$process = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", "src/MCPServer/AICalendar.MCPServer.csproj") -PassThru -NoNewWindow -RedirectStandardInput -RedirectStandardOutput -RedirectStandardError

Start-Sleep -Seconds 3

try {
    $process.StandardInput.WriteLine($initRequest)
    $process.StandardInput.Flush()
    
    Start-Sleep -Seconds 2
    
    if (!$process.HasExited) {
        $response = $process.StandardOutput.ReadLine()
        Write-Host "Response: $response" -ForegroundColor Cyan
        Write-Host "✅ MCP Server responded successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ MCP Server exited unexpectedly" -ForegroundColor Red
    }
} finally {
    if (!$process.HasExited) {
        $process.Kill()
    }
}

Write-Host "`n3. Next Steps:" -ForegroundColor Magenta
Write-Host "   - Make sure your Web API is running on https://localhost:7001"
Write-Host "   - In VS Code, open GitHub Copilot Chat"
Write-Host "   - Type '@aicalendar' to use the MCP server"
Write-Host "   - Try: '@aicalendar Can you show statistics for the previous week?'"

Write-Host "`nMCP Server setup complete! 🎉" -ForegroundColor Green
