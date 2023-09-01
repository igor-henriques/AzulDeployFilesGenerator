namespace AzulDeployFilesGenerator.Application.Factories;

internal sealed class KubernetesDeploymentFactory : IKubernetesDeploymentFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISerializer _serializer;

    public KubernetesDeploymentFactory(
        IOptions<CliCommandOptions> cliOptions)
    {
        _cliOptions = cliOptions;

        _serializer = new SerializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
          .Build();
    }

    public async Task<string> BuildAzulKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = GetEnvVariables(appSettings);
        var k8sDeploy = await GetAzulDeployment(envVariables, cancellationToken);
        return k8sDeploy;
    }

    public async Task<string> BuildOnlineKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = GetEnvVariables(appSettings, getRawValue: true);
        var isabkoDeploy = await GetOnlineDeployment(envVariables, cancellationToken);
        return isabkoDeploy;
    }

    private async Task<string> GetOnlineDeployment(
        List<EnvVariable> variables,
        CancellationToken cancellationToken = default)
    {
        var baseFile = await File.ReadAllTextAsync(
            $"./Files/{_cliOptions.Value.ApplicationType}/isabkodeploy.yaml",
            cancellationToken);

        var serializedVariables = _serializer.Serialize(variables);
        var indentedVariables = IndentYamlString(serializedVariables);

        StringBuilder builder = new(baseFile);

        builder.Replace("$image-name", _cliOptions.Value.IsaBkoImageName);
        builder.Replace("$deploy-name", _cliOptions.Value.DeployName);
        builder.Replace("env:", $"env:\r\n{indentedVariables}");
        builder.Replace("'\"", $"'");
        builder.Replace("\"'", $"'");

        var result = builder.ToString();

        if (!ValidateYaml(result))
        {
            throw new InvalidDataException("Bad deploy yaml file indentation");
        }

        return result;
    }

    private async Task<string> GetAzulDeployment(
        List<EnvVariable> variables,
        CancellationToken cancellationToken = default)
    {
        var baseFile = await File.ReadAllTextAsync(
            $"./Files/{_cliOptions.Value.ApplicationType}/k8sdeploy.yaml",
            cancellationToken);

        var serializedVariables = _serializer.Serialize(variables);
        var indentedVariables = IndentYamlString(serializedVariables);

        StringBuilder builder = new(baseFile);

        builder.Replace("$image-name", _cliOptions.Value.ImageName);
        builder.Replace("$deploy-name", _cliOptions.Value.DeployName);
        builder.Replace("env:", $"env:\r\n{indentedVariables}");
        builder.Replace("'\"", $"'");
        builder.Replace("\"'", $"'");

        var result = builder.ToString();

        if (!ValidateYaml(result))
        {
            throw new InvalidDataException("Bad deploy yaml file indentation");
        }

        return result;
    }

    private string IndentYamlString(string yamlString)
    {
        var indentationLevel = _cliOptions.Value.ApplicationType switch
        {
            EApplicationType.Api => 5,
            EApplicationType.Consumer => 4,
            EApplicationType.CronJob => 8,
            _ => throw new Exception(string.Format(Constants.Messages.INVALID_APPLICATION_TYPE_ERROR_MESSAGE, _cliOptions.Value.ApplicationType))
        };

        var lines = yamlString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var indentedLines = lines.Select(line => new string(' ', indentationLevel * 2) + line); // 2 spaces per indent level
        return string.Join("\n", indentedLines);
    }

    private bool ValidateYaml(string originalYaml)
    {
        try
        {
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(originalYaml));            
            return true;
        }
        catch (Exception)
        {
            return false;
        }        
    }

    private static List<EnvVariable> GetEnvVariables(object obj, string path = "", bool getRawValue = false)
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

            if (prop.GetCustomAttribute<IgnoreDockerTokenization>() != null
                || prop.PropertyType.GetCustomAttribute<IgnoreDockerTokenization>() != null)
            {
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
                    localEnvs.AddRange(GetEnvVariables(listItem, path + jsonPropName + ".", getRawValue: getRawValue));
                }

                result.AddRange(localEnvs);
            }
            else if (prop.PropertyType.IsClass)
            {
                result.AddRange(GetEnvVariables(value, path + jsonPropName + ".", getRawValue: getRawValue));
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
