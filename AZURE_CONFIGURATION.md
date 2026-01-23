# LocalAgent - Azure AI Support

## Configuring AI Provider

LocalAgent supports both local LLMs (via Ollama) and Azure-hosted LLMs (via Azure AI Foundry). The provider is configured through the `AIConfig` section in `LocalAgent.AppHost/appsettings.json`.

### Configuration Structure

Configuration is centralized in `LocalAgent.AppHost/appsettings.json` and automatically passed to the ApiService via environment variables:

```json
{
  "AIConfig": {
    "Provider": "Local",  // Options: "Local" or "Azure"
    "Azure": {
      "Endpoint": "https://your-resource-name.inference.ai.azure.com",
      "ModelId": "llama-3-70b-instruct"
    }
  }
}
```

### Using Local Provider (Default)

By default, LocalAgent uses local Ollama models. To use this configuration:

1. Set `Provider` to `"Local"` in `LocalAgent.AppHost/appsettings.json`
2. The Aspire AppHost will automatically launch Ollama and configure the ApiService
3. The model is specified in the AppHost code (currently "llama3.2")

Example:
```json
{
  "AIConfig": {
    "Provider": "Local"
  }
}
```

### Using Azure AI Foundry

To use Azure-hosted LLMs:

1. Set `Provider` to `"Azure"` in `LocalAgent.AppHost/appsettings.json`
2. Configure your Azure AI Foundry endpoint in `Azure.Endpoint`
3. Specify the model deployment name in `Azure.ModelId`

Example:
```json
{
  "AIConfig": {
    "Provider": "Azure",
    "Azure": {
      "Endpoint": "https://your-resource-name.inference.ai.azure.com",
      "ModelId": "llama-3-70b-instruct"
    }
  }
}
```

#### Authentication

When using Azure AI Foundry, LocalAgent uses `InteractiveBrowserCredential` for authentication. This will:
- Open a browser window for you to sign in with your Azure account
- Cache the credentials for subsequent requests
- Work with Azure AD/Entra ID accounts that have access to the Azure AI resource

Make sure your Azure account has the appropriate permissions to access the Azure AI Foundry resource.

### Benefits of Each Provider

**Local Provider (Ollama)**
- Complete privacy - no data leaves your machine
- No API costs
- Works offline
- Requires GPU hardware for good performance

**Azure Provider**
- Access to more powerful models
- No local GPU required
- Consistent performance
- Pay-per-use pricing

### Architecture Notes

When `Provider` is set to:
- **Local**: The Aspire AppHost will launch an Ollama container and the ApiService will connect to it
- **Azure**: The Aspire AppHost skips launching Ollama, and the ApiService connects directly to Azure AI Foundry

Configuration is centralized in the AppHost's `appsettings.json` and automatically propagated to the ApiService via environment variables, avoiding duplication.
