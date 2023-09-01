namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Retry
{
    [JsonProperty("enabled")]
    public string Enabled { get; init; }

    [JsonProperty("attempts")]
    public string Attempts { get; init; }

    [JsonProperty("waitDuration")]
    public string WaitDuration { get; init; }
}
