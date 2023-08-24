namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record SwaggerDoc
{
    [JsonProperty("name")]
    [IgnoreDockerTokenization]
    public string Name { get; set; }

    [JsonProperty("host")]    
    public string Host { get; set; }

    [JsonProperty("schemes")]
    [IgnoreDockerTokenization]
    public List<string> Schemes { get; set; }

    [JsonProperty("info")]
    [IgnoreDockerTokenization]                                                                          
    public Info Info { get; set; }
}
