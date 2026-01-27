namespace LocalAgent.ServiceDefaults;

public enum AIProvider
{
    Local,
    Azure
}

public class AIConfig
{
    public AIProvider Provider { get; set; } = AIProvider.Local;

    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for HTTP requests to the AI service.
    /// Default is 300 seconds (5 minutes) for local providers to accommodate slower local LLMs.
    /// Note: This timeout applies specifically to HTTP client requests. Some AI providers may have
    /// their own internal timeout mechanisms that are independent of this setting.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Determines if the Azure provider is configured
    /// </summary>
    public bool IsAzureProvider() => Provider == AIProvider.Azure;

    /// <summary>
    /// Determines if the Local provider is configured
    /// </summary>
    public bool IsLocalProvider() => Provider == AIProvider.Local;
}
