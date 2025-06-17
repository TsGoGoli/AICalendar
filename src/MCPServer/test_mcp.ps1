# Test script for the AI Calendar MCP Server
# Run the MCP server and test basic functionality

Write-Host "Testing AI Calendar MCP Server..." -ForegroundColor Green

# Start the MCP server process
$mcpPath = "C:\Users\gogol\Desktop\DataArt\AICalendar\AICalendar\src\MCPServer"
$process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", $mcpPath -NoNewWindow -PassThru -RedirectStandardInput -RedirectStandardOutput -RedirectStandardError

# Wait a moment for the process to start
Start-Sleep -Seconds 2

# Test 1: Initialize
$initMessage = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{
            name = "test-client"
            version = "1.0.0"
        }
    }
} | ConvertTo-Json -Compress

Write-Host "Sending initialize message..." -ForegroundColor Yellow
$process.StandardInput.WriteLine($initMessage)
$process.StandardInput.Flush()

# Read response
$response = $process.StandardOutput.ReadLine()
Write-Host "Initialize response: $response" -ForegroundColor Cyan

# Test 2: List resources
$listResourcesMessage = @{
    jsonrpc = "2.0"
    id = 2
    method = "resources/list"
} | ConvertTo-Json -Compress

Write-Host "Sending list resources message..." -ForegroundColor Yellow
$process.StandardInput.WriteLine($listResourcesMessage)
$process.StandardInput.Flush()

$response = $process.StandardOutput.ReadLine()
Write-Host "List resources response: $response" -ForegroundColor Cyan

# Test 3: List tools
$listToolsMessage = @{
    jsonrpc = "2.0"
    id = 3
    method = "tools/list"
} | ConvertTo-Json -Compress

Write-Host "Sending list tools message..." -ForegroundColor Yellow
$process.StandardInput.WriteLine($listToolsMessage)
$process.StandardInput.Flush()

$response = $process.StandardOutput.ReadLine()
Write-Host "List tools response: $response" -ForegroundColor Cyan

# Clean up
$process.Kill()
Write-Host "MCP Server test completed!" -ForegroundColor Green
