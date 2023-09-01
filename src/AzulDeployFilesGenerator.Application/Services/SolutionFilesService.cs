﻿using AzulDeployFileGenerator.Domain.Models.K8sDeploy;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    private const string CSHARP_CLASS_EXTENSION = "*.cs";
    private const string CSHARP_PROJECT_EXTENSION = "*.sln";
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IKubernetesDeploymentFactory _k8sDeployFactory;

    public SolutionFilesService(
        IOptions<CliCommandOptions> cliOptions,
        IKubernetesDeploymentFactory k8sDeployFactory)
    {
        _cliOptions = cliOptions;
        _k8sDeployFactory = k8sDeployFactory;
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
    /// Searches for a file in a given directory and its subdirectories. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public string GetSolutionName(string relativePath)
    {
        var solutionFiles = Directory.GetFiles(relativePath, CSHARP_PROJECT_EXTENSION, SearchOption.AllDirectories);

        if (solutionFiles.Length > 1)
        {
            throw new InvalidOperationException($".sln duplicates found in the solution path provided. Fix it and try again.");
        }

        var solutionFile = solutionFiles.FirstOrDefault();

        if (string.IsNullOrEmpty(solutionFile) || !File.Exists(solutionFile))
        {
            throw new FileNotFoundException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, ".sln"));
        }

        FileInfo fileInfo = new(solutionFile);

        return fileInfo.Name.Replace(".sln", string.Empty);
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
                        var tokenizedExtraProperties = TokenizeFromExtraProperties(extraProperties);
                        result.Merge(tokenizedExtraProperties);
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

        static JObject TokenizeFromExtraProperties(Dictionary<string, JToken> properties, string parentKey = "")
        {
            JObject result = new();

            foreach (var item in properties)
            {
                string key = string.IsNullOrEmpty(parentKey) ? item.Key : parentKey + "." + item.Key;

                if (item.Value is JObject childObject)
                {
                    // Chamada recursiva para lidar com objetos aninhados
                    JObject childResult = TokenizeFromExtraProperties(childObject.ToObject<Dictionary<string, JToken>>(), key);
                    result.Add(item.Key, childResult); // Adiciona o objeto filho ao pai
                }
                else
                {
                    result.Add(item.Key, "${" + key + "}");
                }
            }

            return result;
        }


    }

    public async Task GenerateAzulK8sDeploy(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {        
        var k8sDeploy = await _k8sDeployFactory.BuildAzulKubernetesDeployment(appSettings, cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.K8sYaml),
            k8sDeploy,
            cancellationToken);
    }

    public async Task GenerateOnlineK8sDeploy(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var isabkoDeploy = await _k8sDeployFactory.BuildOnlineKubernetesDeployment(appSettings, cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.IsaBkoYaml),
            isabkoDeploy.ToString(),
            cancellationToken);
    }
}
