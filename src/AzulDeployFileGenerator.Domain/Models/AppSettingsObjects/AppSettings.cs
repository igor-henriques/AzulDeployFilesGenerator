using Newtonsoft.Json.Linq;

namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record AppSettings
{
    [JsonProperty("log")]
    public Log Log { get; init; }

    [JsonProperty("apiSettings", NullValueHandling = NullValueHandling.Ignore)]
    public ApiSettings ApiSettings { get; init; }

    [JsonProperty("swaggerEndpoint", NullValueHandling = NullValueHandling.Ignore)]
    public SwaggerEndpoint SwaggerEndpoint { get; init; }

    [JsonProperty("swaggerDoc", NullValueHandling = NullValueHandling.Ignore)]
    public SwaggerDoc SwaggerDoc { get; init; }

    [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
    public Resources Resources { get; init; }

    [JsonProperty("connectionSettings", NullValueHandling = NullValueHandling.Ignore)]
    public List<ConnectionSetting> ConnectionSettings { get; init; }

    [JsonProperty("events", NullValueHandling = NullValueHandling.Ignore)]
    public List<Event> Events { get; init; }

    [JsonProperty("serviceBusSettings", NullValueHandling = NullValueHandling.Ignore)]
    public ServiceBusSettings ServiceBusSettings { get; init; }

    [JsonProperty("serviceClients", NullValueHandling = NullValueHandling.Ignore)]
    public List<ServiceClient> ServiceClients { get; init; }

    [JsonExtensionData]    
    public Dictionary<string, JToken> ExtraProperties { get; init; } = new Dictionary<string, JToken>();

    [JsonIgnore]
    [IgnoreDockerTokenization]
    public const string EXTRA_PROPERTIES_NAME = nameof(ExtraProperties);
}
