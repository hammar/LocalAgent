# LocalAgent MCP Server

This MCP (Model Context Protocol) Server provides tools for querying data from various sources, including Microsoft To-Do through the Microsoft Graph API.

## Microsoft To-Do Tool

The To-Do tool allows you to query Microsoft To-Do tasks using the Microsoft Graph API. It supports:
- Getting all task lists
- Getting tasks from a specific list
- Getting tasks due by a specific date

### Authentication

The Microsoft To-Do tool uses **InteractiveBrowserCredential** to authenticate with Microsoft Graph. When you first use the MCP server, a browser window will open asking you to sign in with your Microsoft account (personal or work/school account).

**How it works:**
1. When the MCP server starts and the To-Do tool is first accessed, an interactive browser login prompt appears
2. Sign in with your Microsoft account (personal @outlook.com/@hotmail.com or work/school account)
3. Grant consent for the application to access your To-Do tasks
4. The credentials are cached, so you won't need to sign in again until they expire

The application uses a pre-registered Azure AD app (`LocalAgent`) with ClientId `8a8525ed-8a70-4eeb-9aed-f04448b4764f` that supports both personal and organizational accounts.

### Getting Started

1. **Run the application**:
   ```bash
   cd LocalAgent.McpServer
   dotnet run
   ```

2. **Sign in when prompted**:
   - A browser window will open automatically
   - Sign in with your Microsoft account
   - Grant the requested permissions (`Tasks.Read` and `Tasks.ReadWrite`)

3. **Use the To-Do tools**:
   - Once authenticated, you can query your To-Do tasks through the MCP tools

That's it! No Azure CLI, PowerShell, or IDE configuration needed. Just run and sign in through your browser.

### Permissions

The application requests the following Microsoft Graph permissions:
- `Tasks.Read` - Read your To-Do tasks
- `Tasks.ReadWrite` - Create, read, update, and delete your To-Do tasks

These are the narrowest permissions required for To-Do operations. Additional scopes can be added as new MCP tools are implemented.

## Available Tools

### To-Do Tools

- **GetTaskLists**: Retrieves all To-Do task lists for the authenticated user
- **GetTasksByList**: Gets tasks from a specific list (requires list ID)
- **GetTasksByDueDate**: Gets all tasks due by a specific date across all lists (date format: ISO 8601, e.g., 2024-12-31)

### Weather Tools

- **GetAlerts**: Get weather alerts for a US state
- **GetForecast**: Get weather forecast for a location (latitude/longitude)

### Echo Tool

- **Echo**: Simple echo tool for testing

## Running the Server

```bash
cd LocalAgent.McpServer
dotnet run
```

The server will be available at the configured port with health check endpoint at `/health`.
