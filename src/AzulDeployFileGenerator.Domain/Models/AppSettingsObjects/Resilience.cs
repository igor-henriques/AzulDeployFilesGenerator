namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Resilience
{
    [JsonProperty("circuitbreaker")]
    public CircuitBreaker Circuitbreaker { get; init; }

    [JsonProperty("retry")]
    public Retry Retry { get; init; }
}
