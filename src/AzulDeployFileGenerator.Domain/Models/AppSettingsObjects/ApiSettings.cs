namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record ApiSettings
{
    [JsonProperty("customMiddleware")]
    public CustomMiddleware CustomMiddleware { get; set; }

    [JsonProperty("showDetailedException")]
    public bool ShowDetailedException { get; set; }
}
