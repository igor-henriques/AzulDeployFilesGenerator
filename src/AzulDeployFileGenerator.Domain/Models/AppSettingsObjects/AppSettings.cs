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

    [JsonProperty("resilience", NullValueHandling = NullValueHandling.Ignore)]
    public Resilience Resilience { get; init; }

    [JsonProperty("eventCustomSettings", NullValueHandling = NullValueHandling.Ignore)]
    public EventCustomSettings EventCustomSettings { get; init; }

    [JsonProperty("k8s.schedule", NullValueHandling = NullValueHandling.Ignore)]
    [IgnoreDockerTokenization(includeOnExcel: true)]
    public string K8sSchedule { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> ExtraProperties { get; init; } = new Dictionary<string, JToken>();

    [JsonIgnore]
    [IgnoreDockerTokenization]
    public const string EXTRA_PROPERTIES_NAME = nameof(ExtraProperties);
}

public static class AppSettingsExtensions
{
    /// <summary>
    /// Returns the __tokenized__ version of the appSettings
    /// </summary>
    /// <param name="obj"></param>    
    /// <returns></returns>
    public static List<EnvVariable> GetTokenizedEnvVariables(this AppSettings obj, bool considerExcelExceptionFields = false)
    {
        return GetEnvVariables(obj, false, path: "", considerExcelExceptionFields);
    }

    /// <summary>
    /// Returns the actual value of the properties of the appSettings
    /// </summary>
    /// <param name="obj"></param>    
    /// <returns></returns>
    public static List<EnvVariable> GetRawEnvVariables(this AppSettings obj, bool considerExcelExceptionFields = false)
    {
        return GetEnvVariables(obj, true, path: "", considerExcelExceptionFields);
    }

    private static List<EnvVariable> GetEnvVariables(object obj, bool getRawValue, string path = "", bool considerExcelExceptionFields = false)
    {
        List<EnvVariable> result = new();

        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
            {
                continue;
            }

            if (prop.Name == nameof(AppSettings.ExtraProperties))
            {
                if (prop.GetValue(obj) is Dictionary<string, JToken> extraProperties)
                {
                    AddEnvVariablesFromProperties(extraProperties, result, getRawValue: getRawValue);
                }

                continue;
            }

            JsonPropertyAttribute jsonAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            string jsonPropName = jsonAttribute?.PropertyName ?? prop.Name;
            object value = prop.GetValue(obj);

            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute != null
                && jsonPropertyAttribute.NullValueHandling == NullValueHandling.Ignore
                && value is string or null
                && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                continue;
            }

            var ignoreDockerTokenizationAttributeProp = prop.GetCustomAttribute<IgnoreDockerTokenization>();
            var ignoreDockerTokenizationPropType = prop.PropertyType.GetCustomAttribute<IgnoreDockerTokenization>();

            if (ignoreDockerTokenizationAttributeProp != null || ignoreDockerTokenizationPropType != null)
            {
                if (ignoreDockerTokenizationAttributeProp?.IncludeOnExcel == true
                    && considerExcelExceptionFields)
                {
                    result.Add(new(path + jsonPropName, value?.ToString()));
                }
                
                continue;
            }

            if (jsonPropName.Equals("id", StringComparison.OrdinalIgnoreCase)
                    || (jsonPropName.Equals("key", StringComparison.OrdinalIgnoreCase)))
            {
                path += value.ToString() + ".";
            }
            else if (value is string || value is ValueType)
            {
                string tokenString = path + jsonPropName;

                if (obj.GetType().Name is "Parameter"
                    && jsonPropName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                {
                    tokenString = tokenString[..tokenString.LastIndexOf('.')];
                }

                if (getRawValue)
                {
                    result.Add(new(tokenString, value?.ToString()));
                }
                else
                {
                    result.Add(new(tokenString, $"__{tokenString}__"));
                }
            }
            else if (value is IList list)
            {
                List<EnvVariable> localEnvs = new();

                for (int i = 0; i < list.Count; i++)
                {
                    object listItem = list[i];
                    localEnvs.AddRange(GetEnvVariables(listItem, getRawValue, path + jsonPropName + "."));
                }

                result.AddRange(localEnvs);
            }
            else if (prop.PropertyType.IsClass)
            {
                result.AddRange(GetEnvVariables(value, getRawValue, path + jsonPropName + "."));
            }
        }

        return result;
    }

    private static void AddEnvVariablesFromProperties(
            Dictionary<string, JToken> properties,
            List<EnvVariable> result,
            string parentKey = "",
            bool getRawValue = false)
    {
        foreach (var item in properties)
        {
            string key = string.IsNullOrEmpty(parentKey) ? item.Key : parentKey + "." + item.Key;

            if (item.Value is JObject childObject)
            {
                AddEnvVariablesFromProperties(childObject.ToObject<Dictionary<string, JToken>>(), result, key, getRawValue: getRawValue);
            }
            else
            {
                if (getRawValue)
                {
                    result.Add(new EnvVariable(key, item.Value?.ToString()));
                }
                else
                {
                    result.Add(new EnvVariable(key, $"__{key}__"));
                }
            }
        }
    }
}