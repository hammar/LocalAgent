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
      "ModelId": "llama-3-70b-instruct",
      "ApiKey": "your-api-key-here"
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
4. Store your API key securely using .NET user-secrets

#### Storing the API Key Securely

For local development, use .NET user-secrets to store the API key securely:

```bash
# Navigate to the AppHost project directory
cd LocalAgent.AppHost

# Set the API key using user-secrets
dotnet user-secrets set "AIConfig:Azure:ApiKey" "your-actual-api-key-here"
```

User-secrets are stored outside the project directory and are not committed to source control, keeping your API key secure.

Example appsettings.json (without the API key):
```json
{
  "AIConfig": {
    "Provider": "Azure",
    "Azure": {
      "Endpoint": "https://your-resource-name.inference.ai.azure.com",
      "ModelId": "llama-3-70b-instruct"
      // ApiKey is read from user-secrets, not stored here
    }
  }
}
```

#### Authentication

LocalAgent uses API key authentication (`AzureKeyCredential`) to connect to Azure AI Foundry. The API key is:
- Stored securely in .NET user-secrets for local development
- Automatically picked up by the configuration system
- Passed securely to the ApiService via environment variables

Make sure you have a valid API key from your Azure AI Foundry resource.

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
- **Azure**: The Aspire AppHost skips launching Ollama, and the ApiService connects directly to Azure AI Foundry using API key authentication

Configuration is centralized in the AppHost's `appsettings.json` (with sensitive values in user-secrets) and automatically propagated to the ApiService via environment variables, avoiding duplication.
