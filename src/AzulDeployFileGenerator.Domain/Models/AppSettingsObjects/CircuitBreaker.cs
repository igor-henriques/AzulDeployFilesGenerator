namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record CircuitBreaker
{
    [JsonProperty("enabled")]
    public string Enabled { get; init; }

    [JsonProperty("failureThreshold")]
    public string FailureThreshold { get; init; }

    [JsonProperty("samplingDuration")]
    public string SamplingDuration { get; init; }

    [JsonProperty("minimumThrouput")]
    public string MinimumThrouput { get; init; }

    [JsonProperty("durationOfBreak")]
    public string DurationOfBreak { get; init; }
}