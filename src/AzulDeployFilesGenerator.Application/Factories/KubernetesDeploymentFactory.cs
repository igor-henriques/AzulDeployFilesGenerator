namespace AzulDeployFilesGenerator.Application.Factories;

internal sealed class KubernetesDeploymentFactory : IKubernetesDeploymentFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IOptions<ApplicationDefaultsOptions> _appDefaultsOptions;
    private readonly ISerializer _serializer;

    public KubernetesDeploymentFactory(
        IOptions<CliCommandOptions> cliOptions,
        IOptions<ApplicationDefaultsOptions> appDefaultsOptions)
    {
        _cliOptions = cliOptions;

        _serializer = new SerializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
          .Build();
        _appDefaultsOptions = appDefaultsOptions;
    }

    public async Task<string> BuildAzulKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = appSettings.GetTokenizedEnvVariables();
        var k8sDeploy = await GetAzulDeployment(envVariables, cancellationToken);
        return k8sDeploy;
    }

    public async Task<string> BuildOnlineKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = appSettings.GetRawEnvVariables();
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

        builder.Replace("$namespace", _appDefaultsOptions.Value.OnlineKubernetesNamespace);
        builder.Replace("$image-name", _cliOptions.Value.IsaBkoImageName);
        builder.Replace("$deploy-name", _cliOptions.Value.DeployName);
        builder.Replace("env:", $"env:\r\n{indentedVariables}");
        builder.Replace("'\"", $"'");
        builder.Replace("\"'", $"'");

        var result = builder.ToString();

        if (!ValidateYaml(result))
        {
            throw new ApplicationException("Bad deploy yaml file indentation");
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
            throw new ApplicationException("Bad deploy yaml file indentation");
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

    private static bool ValidateYaml(string originalYaml)
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
}
