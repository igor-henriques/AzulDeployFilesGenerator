namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record EventCustomSettings
{
    [JsonProperty("AzureServiceBusSettings", NullValueHandling = NullValueHandling.Ignore)]
    public AzureServiceBusSettings AzureServiceBusSettings { get; init; }
}
