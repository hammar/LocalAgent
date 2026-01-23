namespace LocalAgent.ServiceDefaults;

public class AIConfig
{
    public string Provider { get; set; } = "Local"; // Options: "Local" or "Azure"
    public AzureConfig Azure { get; set; } = new();
    public LocalConfig Local { get; set; } = new();
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
