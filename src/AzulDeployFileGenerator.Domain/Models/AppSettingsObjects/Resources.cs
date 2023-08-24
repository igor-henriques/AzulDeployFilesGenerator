namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

[IgnoreDockerTokenization]
public sealed record Resources
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("cacheExpirationMinutes")]
    public int CacheExpirationMinutes { get; set; }
}
