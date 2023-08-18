namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ServiceClient
{
    [JsonProperty("id")]
    public string Id { get; init; }

    [JsonProperty("address")]
    public string Address { get; init; }

    [JsonProperty("timeout")]
    public int Timeout { get; init; }

    [JsonProperty("parameters")]
    public List<Parameter> Parameters { get; init; }

    [JsonIgnore]
    public string FormattedIdForSearchingInClasses => $"public override string ServiceClientId => \"{Id}\";";
}