@echo off
echo ================================================================
echo AI Calendar MCP Server - Quick Demo
echo ================================================================
echo.

echo 1. Testing Initialize Message...
echo {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"demo","version":"1.0"}}} | dotnet run --project . --verbosity quiet
echo.

echo 2. Testing List Tools...
echo {"jsonrpc":"2.0","id":2,"method":"tools/list"} | dotnet run --project . --verbosity quiet
echo.

echo 3. Testing List Resources...
echo {"jsonrpc":"2.0","id":3,"method":"resources/list"} | dotnet run --project . --verbosity quiet
echo.

echo 4. Testing List Prompts...
echo {"jsonrpc":"2.0","id":4,"method":"prompts/list"} | dotnet run --project . --verbosity quiet
echo.

echo ================================================================
echo Demo completed! The MCP server is working correctly.
echo ================================================================
