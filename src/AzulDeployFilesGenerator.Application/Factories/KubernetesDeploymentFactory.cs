namespace AzulDeployFilesGenerator.Application.Factories;

/// <summary>
/// Factory for generating Kubernetes deployment files tailored for Azul and Online environments.
/// This factory uses application settings and CLI options to populate environment variables and other settings in the deployment files.
/// </summary>
internal sealed class KubernetesDeploymentFactory : IKubernetesDeploymentFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IOptions<ApplicationDefaultsOptions> _appDefaultsOptions;
    private readonly ISerializer _serializer;

    /// <summary>
    /// Constructor that initializes CLI options, Application Default options, and configures the YAML serializer.
    /// </summary>
    /// <param name="cliOptions">CLI options passed to the application.</param>
    /// <param name="appDefaultsOptions">Default application settings.</param>    
    /// <exception cref="ArgumentNullException">Thrown if either <paramref name="cliOptions"/> or <paramref name="appDefaultsOptions"/> is null.</exception>
    public KubernetesDeploymentFactory(
        IOptions<CliCommandOptions> cliOptions,
        IOptions<ApplicationDefaultsOptions> appDefaultsOptions)
    {
        _cliOptions = cliOptions ?? throw new ArgumentNullException(nameof(cliOptions));
        _appDefaultsOptions = appDefaultsOptions ?? throw new ArgumentNullException(nameof(appDefaultsOptions));

        _serializer = new SerializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
          .Build();        
    }

    /// <summary>
    /// Builds a Kubernetes deployment file specifically for the Azul environment.
    /// </summary>
    /// <param name="appSettings">Application settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Azul Kubernetes deployment file.</returns>
    public async Task<string> BuildAzulKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = appSettings.GetTokenizedEnvVariables();
        var k8sDeploy = await GetAzulDeployment(envVariables, cancellationToken);
        return k8sDeploy;
    }

    /// <summary>
    /// Builds a Kubernetes deployment file specifically for the Online environment.
    /// </summary>
    /// <param name="appSettings">Application settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Online Kubernetes deployment file.</returns>
    public async Task<string> BuildOnlineKubernetesDeployment(
        AppSettings appSettings,
        CancellationToken cancellationToken = default)
    {
        var envVariables = appSettings.GetRawEnvVariables();
        var isabkoDeploy = await GetOnlineDeployment(envVariables, cancellationToken);
        return isabkoDeploy;
    }

    /// <summary>
    /// Generates a Kubernetes deployment file for the Online environment by replacing placeholders in the base file.
    /// </summary>
    /// <param name="variables">Environment variables.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Online Kubernetes deployment file.</returns>
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

    /// <summary>
    /// Generates a Kubernetes deployment file for the Azul environment by replacing placeholders in the base file.
    /// </summary>
    /// <param name="variables">Environment variables.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Azul Kubernetes deployment file.</returns>
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

    /// <summary>
    /// Indents a YAML string according to the application type.
    /// </summary>
    /// <param name="yamlString">The original YAML string.</param>
    /// <returns>An indented YAML string.</returns>
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

    /// <summary>
    /// Validates the generated YAML string to ensure it is well-formed.
    /// </summary>
    /// <param name="originalYaml">The YAML string to validate.</param>
    /// <returns>True if the YAML is valid; otherwise, false.</returns>
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
