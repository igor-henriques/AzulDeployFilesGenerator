namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record AppSettings
{
    [JsonProperty("log")]
    public Log Log { get; init; }

    [JsonProperty("events")]
    public List<Event> Events { get; init; }

    [JsonProperty("serviceBusSettings")]
    public ServiceBusSettings ServiceBusSettings { get; init; }

    [JsonProperty("serviceClients")]
    public List<ServiceClient> ServiceClients { get; init; }
}
