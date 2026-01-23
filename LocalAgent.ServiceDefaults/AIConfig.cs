namespace LocalAgent.ServiceDefaults;

public class AIConfig
{
    public string Provider { get; set; } = "Local"; // Options: "Local" or "Azure"
    public AzureConfig Azure { get; set; } = new();
    public LocalConfig Local { get; set; } = new();

    /// <summary>
    /// Determines if the Azure provider is configured
    /// </summary>
    public bool IsAzureProvider() => string.Equals(Provider, "Azure", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the Local provider is configured
    /// </summary>
    public bool IsLocalProvider() => !IsAzureProvider();
}

public class AzureConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
}

public class LocalConfig
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "llama3.2";
}
