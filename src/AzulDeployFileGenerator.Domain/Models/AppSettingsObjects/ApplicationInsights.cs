namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ApplicationInsights
{
    [JsonProperty("enabled")]
    public bool Enabled { get; init; }

    [JsonProperty("minimumLevel")]
    public string MinimumLevel { get; init; }

    [JsonProperty("instrumentationKey")]
    public string InstrumentationKey { get; init; }
}
