namespace LocalAgent.ServiceDefaults;

public enum AIProvider
{
    Local,
    Azure
}

public class AIConfig
{
    public AIProvider Provider { get; set; } = AIProvider.Local;
    public AzureConfig Azure { get; set; } = new();

    /// <summary>
    /// Determines if the Azure provider is configured
    /// </summary>
    public bool IsAzureProvider() => Provider == AIProvider.Azure;

    /// <summary>
    /// Determines if the Local provider is configured
    /// </summary>
    public bool IsLocalProvider() => Provider == AIProvider.Local;
}

public class AzureConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
}
