namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record AzureServiceBusSettings
{
    [JsonProperty("autoComplete")]
    public bool AutoComplete { get; init; }

    [JsonProperty("isSession")]
    public bool IsSession { get; init; }

    [JsonProperty("maxAutoRenewDuration")]
    public string MaxAutoRenewDuration { get; init; }

    [JsonProperty("maxConcurrentCalls")]
    public int MaxConcurrentCalls { get; init; }
}
