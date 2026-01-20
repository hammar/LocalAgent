# LocalAgent MCP Server

This MCP (Model Context Protocol) Server provides tools for querying data from various sources, including Microsoft To-Do through the Microsoft Graph API.

## Microsoft To-Do Tool

The To-Do tool allows you to query Microsoft To-Do tasks using the Microsoft Graph API. It supports:
- Getting all task lists
- Getting tasks from a specific list
- Getting tasks due by a specific date

### Authentication

The Microsoft To-Do tool uses **DefaultAzureCredential** to authenticate with Microsoft Graph. This means it will automatically use your existing Azure credentials without requiring any configuration or Entra app registration.

**Supported authentication methods** (tried in this order):
1. **Azure CLI** - Run `az login` to authenticate
2. **Azure PowerShell** - Run `Connect-AzAccount` to authenticate
3. **Visual Studio** - Sign in through Tools → Options → Azure Service Authentication
4. **VS Code** - Sign in through the Azure Account extension
5. **Managed Identity** - If deployed to Azure (e.g., Azure App Service, Azure Functions)
6. **Environment variables** - For CI/CD scenarios

### Getting Started

1. **Install Azure CLI** (recommended for local development):
   - Download from [https://docs.microsoft.com/cli/azure/install-azure-cli](https://docs.microsoft.com/cli/azure/install-azure-cli)
   - Run `az login` and follow the prompts to sign in with your Azure account

2. **Ensure you have access to Microsoft To-Do**:
   - Your Azure account must have access to Microsoft To-Do in your tenant
   - The application requests `Tasks.Read` and `Tasks.ReadWrite` permissions

3. **Run the application**:
   ```bash
   cd LocalAgent.McpServer
   dotnet run
   ```

The application will automatically detect your Azure credentials and authenticate with Microsoft Graph.

### Alternative Authentication Methods

If you prefer not to use Azure CLI:

- **Visual Studio**: Sign in through Tools → Options → Azure Service Authentication
- **VS Code**: Install the Azure Account extension and sign in
- **Azure PowerShell**: Run `Connect-AzAccount`

No configuration files or app registrations are needed!

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
