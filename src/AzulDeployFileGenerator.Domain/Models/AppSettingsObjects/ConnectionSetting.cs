namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ConnectionSetting
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("connectionString")]
    public string ConnectionString { get; set; }

    [JsonProperty("databaseName", NullValueHandling = NullValueHandling.Ignore)]
    public string DatabaseName { get; set; }
}
