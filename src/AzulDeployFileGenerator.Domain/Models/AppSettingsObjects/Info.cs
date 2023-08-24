namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record Info
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}
