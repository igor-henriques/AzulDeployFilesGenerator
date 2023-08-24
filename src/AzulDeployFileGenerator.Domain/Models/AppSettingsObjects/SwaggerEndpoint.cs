namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

[IgnoreDockerTokenization]
public sealed record SwaggerEndpoint
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}
