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

3. **Enable On-Behalf-Of Flow** (for production):
   - In your app registration, go to "Expose an API"
   - Click "Add a scope" to define at least one scope (if not already done)
   - Note: The calling application (Web/ApiService) will need to be configured to request tokens for this API

4. **Create a Client Secret** (for production deployments):
   - Go to "Certificates & secrets"
   - Click "New client secret"
   - Provide a description and expiration
   - Copy the secret value (you won't be able to see it again!)

5. **Note your Application Details**:
   - Copy the **Application (client) ID** from the Overview page
   - Copy the **Directory (tenant) ID** from the Overview page

### Configuration

The application automatically detects the authentication mode based on configuration and request context:

**Local Development Mode:**
- Triggered when `TenantId` or `ClientId` are empty/missing in configuration
- Uses **DefaultAzureCredential** which tries authentication methods in this order:
  1. Environment variables (useful for CI/CD)
  2. Managed Identity (when deployed to Azure)
  3. Visual Studio/VS Code (automatically uses your IDE login)
  4. Azure CLI (`az login`)
  5. Interactive browser (as a fallback)
- No configuration required in `appsettings.json`

**Production Mode with On-Behalf-Of (OBO) Flow:**
- Triggered when `TenantId` and `ClientId` are configured AND a user token is provided via the `Authorization` header
- Uses **OnBehalfOfCredential** to exchange the user's token for a Microsoft Graph token
- The application acts on behalf of the authenticated user, using their identity and permissions
- Requires: `TenantId`, `ClientId`, `ClientSecret`, and a valid user bearer token in the request
- This is the **required production mode** - requests without a valid user token will fail with an error
- **Security**: Prevents unauthorized access by ensuring operations are performed with the user's permissions only

> **Important Security Note**: In production (when TenantId and ClientId are configured), a user token **must** be provided in the Authorization header. The application will throw an error if no user token is present, preventing a security risk where the application might use elevated app-only permissions that could grant users access to data beyond their individual permissions.

### Production Configuration

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

> **Security Note:** Never store secrets in `appsettings.json` in production. Use Azure Key Vault, environment variables, or user secrets instead.

### On-Behalf-Of (OBO) Flow

For production deployments where you want to act on behalf of the authenticated user:

1. **Configure the Entra App** with the settings above

2. **Set up Authentication in the Calling Application**: 
   - The calling application (e.g., Web or ApiService) must implement user authentication (e.g., using Microsoft.Identity.Web or similar)
   - The calling application should obtain an access token for the authenticated user
   - When making HTTP requests to the MCP server, include the user's access token in the `Authorization` header:
     ```
     Authorization: Bearer <user-access-token>
     ```

3. **The MCP server will**:
   - Validate that the Authorization header is present (in production mode)
   - Extract the user token from the header
   - Use the OBO flow to exchange it for a Microsoft Graph token
   - All Microsoft Graph operations will be performed with the user's identity and permissions

4. **If no Authorization header is present** in production mode (when TenantId/ClientId are configured):
   - The request will fail with an `InvalidOperationException`
   - This is a security feature to prevent unauthorized access

> **Note**: Currently, the MCP server does not implement authentication middleware itself. The calling application is responsible for authenticating users and passing their tokens. Future enhancements could add authentication middleware to the MCP server for defense-in-depth.

### Local Development

The application automatically uses local development mode when configuration values are not provided.

To use local development authentication:

1. Install [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
2. Run `az login` to authenticate
3. Ensure your account has access to Microsoft To-Do in your tenant

The application will automatically use your local credentials without needing to configure TenantId, ClientId, or ClientSecret in `appsettings.json`.

Alternative local authentication methods (no additional setup required if you're already signed in):
- Visual Studio: Sign in through Tools → Options → Azure Service Authentication
- VS Code: Sign in through the Azure Account extension

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
