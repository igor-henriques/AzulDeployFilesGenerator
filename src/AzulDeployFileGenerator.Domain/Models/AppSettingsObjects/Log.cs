namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Log
{
    [JsonProperty("console")]
    public Console Console { get; init; }

    [JsonProperty("applicationInsights")]
    public ApplicationInsights ApplicationInsights { get; init; }
}