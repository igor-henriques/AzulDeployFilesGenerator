using AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;
using AzulDeployFileGenerator.Domain.Models.Cli;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

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
    public async Task<bool> AnyClassContainsString(string relativePath, string content, CancellationToken cancellationToken = default)
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
    /// Generates a tokenized version of the appsettings, stands for appsettings.Docker.json
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateAppSettingsDocker(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var appSettingsDockerPath = Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.AppSettingsDocker);
        var tokenizedAppSettings = Tokenize(appSettings);

        await File.WriteAllTextAsync(appSettingsDockerPath, tokenizedAppSettings.ToString(), cancellationToken);
    }

    private static JObject Tokenize(object obj, string path = "")
    {
        JObject result = new();
        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                continue;

            JsonPropertyAttribute jsonAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonAttribute == null)
                continue;

            string jsonPropName = jsonAttribute.PropertyName;
            object value = prop.GetValue(obj);

            if (jsonPropName.Equals("id", StringComparison.OrdinalIgnoreCase) || jsonPropName.Equals("key", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(jsonPropName, JToken.FromObject(value));
                path += value.ToString() + ".";
            }
            else if (value is string || value is ValueType)
            {
                string tokenString = path + jsonPropName;
                result.Add(jsonPropName, "${" + tokenString + "}");
            }
            else if (value is IList list)
            {
                JArray array = new();
                for (int i = 0; i < list.Count; i++)
                {
                    object listItem = list[i];
                    array.Add(Tokenize(listItem, path + jsonPropName + "."));
                }
                result.Add(jsonPropName, array);
            }
            else if (prop.PropertyType.IsClass)
            {
                result.Add(jsonPropName, Tokenize(value, path + jsonPropName + "."));
            }
        }
        return result;
    }
}
