using AzulDeployFileGenerator.Domain.Attributes;

namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    private const string CSHARP_CLASS_EXTENSION = "*.cs";
    private readonly IOptions<CliCommandOptions> _cliOptions;

    public SolutionFilesService(
        IOptions<CliCommandOptions> cliOptions)
    {
        _cliOptions = cliOptions;
    }

    /// <summary>
    /// Searches for a file in a given directory and its subdirectories. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async ValueTask<string> GetFileContent(string relativePath, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Directory.GetFiles(relativePath, fileName, SearchOption.AllDirectories).FirstOrDefault();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, fileName));
        }

        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    /// <summary>
    /// Search for a specific string content in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<bool> AnySolutionClassContainsText(string relativePath, string content, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(relativePath, CSHARP_CLASS_EXTENSION, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var classRawText = await File.ReadAllTextAsync(file, cancellationToken);

            if (classRawText.Contains(content))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Generates the appsettings.json file for the online environment.
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateAppSettingsOnline(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var appSettingsOnlinePath = Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.AppSettingsOnline);
        var appSettingsString = JsonConvert.SerializeObject(appSettings);

        await File.WriteAllTextAsync(appSettingsOnlinePath, appSettingsString, cancellationToken);
    }

    /// <summary>
    /// Generates a tokenized version of the appsettings (appsettings.Docker.json)
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateAppSettingsDocker(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var appSettingsDockerPath = Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.AppSettingsDocker);
        var tokenizedAppSettings = TokenizeAppSettings(appSettings);

        await File.WriteAllTextAsync(appSettingsDockerPath, tokenizedAppSettings.ToString(), cancellationToken);

        static JObject TokenizeAppSettings(object obj, string path = "")
        {
            if (obj.GetType().GetCustomAttribute<IgnoreDockerTokenization>() != null)
            {
                return JToken.FromObject(obj) as JObject;
            }

            JObject result = new();
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
                        foreach (var item in extraProperties)
                        {
                            result.Add(item.Key, item.Value);
                        }
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

                if (prop.GetCustomAttribute<IgnoreDockerTokenization>() != null)
                {
                    result.Add(jsonPropName, JToken.FromObject(value)); 
                    continue;
                }

                if (jsonPropName.Equals("id", StringComparison.OrdinalIgnoreCase) 
                    || (jsonPropName.Equals("key", StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(jsonPropName, JToken.FromObject(value)); 
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

                    result.Add(jsonPropName, "${" + tokenString + "}");
                }
                else if (value is IList list)
                {
                    JArray array = new();

                    for (int i = 0; i < list.Count; i++)
                    {
                        object listItem = list[i];
                        array.Add(TokenizeAppSettings(listItem, path + jsonPropName + "."));
                    }

                    result.Add(jsonPropName, array);
                }
                else if (prop.PropertyType.IsClass)
                {
                    result.Add(jsonPropName, TokenizeAppSettings(value, path + jsonPropName + "."));
                }
            }

            return result;
        }
    }    
}
