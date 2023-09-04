namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ServiceClient
{
    [JsonProperty("id")]
    public string Id { get; init; }

    [JsonProperty("address")]
    public string Address { get; init; }

    [JsonProperty("timeout")]
    public int Timeout { get; init; }

    [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
    public List<Parameter> Parameters { get; init; }

    [JsonIgnore]
    public string FormattedIdForSearchingInSolutionClasses => $"public override string ServiceClientId => \"{Id}\";";
}