namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Console
{
    [JsonProperty("enabled")]
    public bool Enabled { get; init; }

    [JsonProperty("minimumLevel")]
    public string MinimumLevel { get; init; }
}