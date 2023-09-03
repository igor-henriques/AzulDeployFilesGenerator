namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Event
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("transportType")]
    public string TransportType { get; set; }

    [JsonProperty("connectionString")]
    public string ConnectionString { get; set; }

    [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
    public List<Parameter> Parameters { get; set; }
}
