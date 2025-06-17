#!/usr/bin/env python3
"""
Comprehensive test script for the AI Calendar MCP Server
"""
import json
import subprocess
import sys
import time

def send_mcp_message(process, message):
    """Send a JSON-RPC message to the MCP server"""
    json_msg = json.dumps(message) + '\n'
    print(f">>> Sending: {json_msg.strip()}")
    process.stdin.write(json_msg.encode())
    process.stdin.flush()
    
    # Read response
    response_line = process.stdout.readline().decode().strip()
    if response_line:
        response = json.loads(response_line)
        print(f"<<< Received: {json.dumps(response, indent=2)}")
        return response
    return None

def comprehensive_test():
    """Test the MCP server comprehensive functionality"""
    print("=" * 60)
    print("Starting Comprehensive AI Calendar MCP Server Test")
    print("=" * 60)
    
    # Start the MCP server process
    mcp_path = r"C:\Users\gogol\Desktop\DataArt\AICalendar\AICalendar\src\MCPServer"
    
    try:
        process = subprocess.Popen(
            ["dotnet", "run", "--project", mcp_path],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            cwd=mcp_path
        )
        
        # Wait a moment for the server to start
        time.sleep(2)
        
        # Test 1: Initialize
        print("\n1. Testing Initialize...")
        init_message = {
            "jsonrpc": "2.0",
            "id": 1,
            "method": "initialize",
            "params": {
                "protocolVersion": "2024-11-05",
                "capabilities": {},
                "clientInfo": {
                    "name": "comprehensive-test-client",
                    "version": "1.0.0"
                }
            }
        }
        
        response = send_mcp_message(process, init_message)
        if not response or response.get("error"):
            print("❌ Initialize failed!")
            return False
        print("✅ Initialize successful!")
        
        # Test 2: List resources
        print("\n2. Testing List Resources...")
        list_resources_message = {
            "jsonrpc": "2.0",
            "id": 2,
            "method": "resources/list"
        }
        
        response = send_mcp_message(process, list_resources_message)
        if not response or response.get("error"):
            print("❌ List resources failed!")
            return False
        print("✅ List resources successful!")
        
        # Test 3: List tools
        print("\n3. Testing List Tools...")
        list_tools_message = {
            "jsonrpc": "2.0",
            "id": 3,
            "method": "tools/list"
        }
        
        response = send_mcp_message(process, list_tools_message)
        if not response or response.get("error"):
            print("❌ List tools failed!")
            return False
        print("✅ List tools successful!")
        
        # Test 4: Read events resource
        print("\n4. Testing Read Events Resource...")
        read_events_message = {
            "jsonrpc": "2.0",
            "id": 4,
            "method": "resources/read",
            "params": {
                "uri": "calendar://events"
            }
        }
        
        response = send_mcp_message(process, read_events_message)
        if not response or response.get("error"):
            print("❌ Read events resource failed!")
            return False
        print("✅ Read events resource successful!")
        
        # Test 5: Create event tool
        print("\n5. Testing Create Event Tool...")
        create_event_message = {
            "jsonrpc": "2.0",
            "id": 5,
            "method": "tools/call",
            "params": {
                "name": "create_event",
                "arguments": {
                    "title": "MCP Test Meeting",
                    "description": "A test meeting created via MCP",
                    "start": "2024-06-25T14:00:00Z",
                    "end": "2024-06-25T15:00:00Z",
                    "organizerId": "00000000-0000-0000-0000-000000000001"
                }
            }
        }
        
        response = send_mcp_message(process, create_event_message)
        if not response or response.get("error"):
            print("❌ Create event tool failed!")
            return False
        print("✅ Create event tool successful!")
        
        # Test 6: List prompts
        print("\n6. Testing List Prompts...")
        list_prompts_message = {
            "jsonrpc": "2.0",
            "id": 6,
            "method": "prompts/list"
        }
        
        response = send_mcp_message(process, list_prompts_message)
        if not response or response.get("error"):
            print("❌ List prompts failed!")
            return False
        print("✅ List prompts successful!")
        
        # Close the process
        process.terminate()
        process.wait()
        
        print("\n" + "=" * 60)
        print("✅ ALL TESTS PASSED! MCP Server is working correctly!")
        print("=" * 60)
        return True
        
    except Exception as e:
        print(f"❌ Error during testing: {e}")
        return False

if __name__ == "__main__":
    success = comprehensive_test()
    sys.exit(0 if success else 1)
