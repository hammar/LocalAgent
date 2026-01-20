# LocalAgent MCP Server

This MCP (Model Context Protocol) Server provides tools for querying data from various sources, including Microsoft To-Do through the Microsoft Graph API.

## Microsoft To-Do Tool Configuration

The To-Do tool allows you to query Microsoft To-Do tasks using the Microsoft Graph API. It supports:
- Getting all task lists
- Getting tasks from a specific list
- Getting tasks due by a specific date

### Prerequisites

To use the Microsoft To-Do tool, you need:

1. An **Azure Entra ID (formerly Azure AD) application** registered in your tenant
2. The application must have the following **Microsoft Graph API permissions**:
   - `Tasks.Read` (for read-only access)
   - `Tasks.ReadWrite` (for future write operations)

### Setting up Azure Entra ID Application

1. **Register an Application in Azure Portal**:
   - Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
   - Click "New registration"
   - Provide a name (e.g., "LocalAgent MCP Server")
   - Select supported account types (typically "Accounts in this organizational directory only")
   - Click "Register"

2. **Configure API Permissions**:
   - In your app registration, go to "API permissions"
   - Click "Add a permission" → "Microsoft Graph" → "Delegated permissions"
   - Add `Tasks.Read` and `Tasks.ReadWrite`
   - Click "Grant admin consent" (if you have admin rights)

3. **Create a Client Secret** (for production deployments):
   - Go to "Certificates & secrets"
   - Click "New client secret"
   - Provide a description and expiration
   - Copy the secret value (you won't be able to see it again!)

4. **Note your Application Details**:
   - Copy the **Application (client) ID** from the Overview page
   - Copy the **Directory (tenant) ID** from the Overview page

### Configuration

Update the `appsettings.json` or use environment variables/user secrets to configure:

```json
{
  "MicrosoftGraph": {
    "TenantId": "your-tenant-id-here",
    "ClientId": "your-client-id-here",
    "ClientSecret": "your-client-secret-here",
    "Scopes": [
      "https://graph.microsoft.com/.default"
    ]
  }
}
```

> **Note:** The empty values in `appsettings.json` are intentional. These values are **only required for production deployments** where you cannot use DefaultAzureCredential. For local development, DefaultAzureCredential will automatically use your Azure CLI or Visual Studio credentials without requiring these values to be configured.

**For production**: Use Azure Key Vault, environment variables, or other secure configuration methods instead of storing secrets in `appsettings.json`.

### Local Development

For local development, the tool uses **DefaultAzureCredential**, which automatically tries multiple authentication methods:

1. **Environment variables** (useful for CI/CD)
2. **Managed Identity** (when deployed to Azure)
3. **Visual Studio** (automatically uses your VS login)
4. **Azure CLI** (`az login`)
5. **Interactive browser** (as a fallback)

To use local development authentication:

1. Install [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
2. Run `az login` to authenticate
3. Ensure your account has access to Microsoft To-Do in your tenant

The application will automatically use your local credentials without needing to configure TenantId, ClientId, or ClientSecret in `appsettings.json`.

### Testing in Different Tenants

To test with your own tenant and Entra app:

1. Create your own Azure Entra ID application (see steps above)
2. Update the configuration values in `appsettings.json` or use user secrets:
   ```bash
   dotnet user-secrets set "MicrosoftGraph:TenantId" "your-tenant-id"
   dotnet user-secrets set "MicrosoftGraph:ClientId" "your-client-id"
   dotnet user-secrets set "MicrosoftGraph:ClientSecret" "your-client-secret"
   ```

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
