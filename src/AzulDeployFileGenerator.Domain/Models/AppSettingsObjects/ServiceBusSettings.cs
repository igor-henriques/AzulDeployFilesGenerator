namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ServiceBusSettings
{
    [JsonProperty("publisherTopic")]
    public string PublisherTopic { get; init; }

    [JsonProperty("autoComplete")]
    public bool AutoComplete { get; init; }

    [JsonProperty("maxConcurrentCalls")]
    public int MaxConcurrentCalls { get; init; }
}