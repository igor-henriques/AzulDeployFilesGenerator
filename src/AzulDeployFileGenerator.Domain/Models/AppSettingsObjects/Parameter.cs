namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Parameter
{
    [JsonProperty("key")]
    public string Key { get; init; }

    [JsonProperty("value")]
    public string Value { get; init; }
}