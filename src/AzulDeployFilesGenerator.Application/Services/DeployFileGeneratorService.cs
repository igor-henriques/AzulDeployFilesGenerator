namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class DeployFileGeneratorService : IDeployFileGeneratorService
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IKubernetesDeploymentFactory _k8sDeployFactory;
    private readonly ITokenizedAppSettingsFactory _tokenizedAppSettingsFactory;    
    private readonly IExcelSheetFactory _excelSheetFactory;
    private readonly IDockerfileFactory _dockerfileFactory;

    public DeployFileGeneratorService(IOptions<CliCommandOptions> cliOptions,
                                      IKubernetesDeploymentFactory k8sDeployFactory,
                                      IExcelSheetFactory excelSheetFactory,
                                      ITokenizedAppSettingsFactory tokenizedAppSettingsFactory,
                                      IDockerfileFactory dockerfileFactory)
    {
        _cliOptions = cliOptions;
        _k8sDeployFactory = k8sDeployFactory;
        _excelSheetFactory = excelSheetFactory;
        _tokenizedAppSettingsFactory = tokenizedAppSettingsFactory;
        _dockerfileFactory = dockerfileFactory;
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
        var tokenizedAppSettings = _tokenizedAppSettingsFactory.BuildTokenizedAppSettingsAsync(appSettings, cancellationToken);

        await File.WriteAllTextAsync(
            appSettingsDockerPath, 
            tokenizedAppSettings, 
            cancellationToken);
    }

    /// <summary>
    /// Generates k8sdeploy.yaml file
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Generates isabkodeploy.yaml file
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Generates Dockerfile for Azul environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateAzulDockerfile(CancellationToken cancellationToken = default)
    {
        var dockerfile = await _dockerfileFactory.BuildAzulDockerfile(cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.Dockerfile),
            dockerfile.ToString(),
            cancellationToken);
    }

    /// <summary>
    /// Generates Dockerfile for Online environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateOnlineDockerfile(CancellationToken cancellationToken = default)
    {
        var dockerfile = await _dockerfileFactory.BuildOnlineDockerfile(cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(_cliOptions.Value.OutputPath, Constants.FileNames.DockerfileOnline),
            dockerfile,
            cancellationToken);
    }

    /// <summary>
    /// Generates a tokenized Excel version of the appsettings, following Azul Airlines guidelines
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateTokenizationExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        using var excel = await _excelSheetFactory.BuildExcelSheet(appSettings, cancellationToken);  
        
        var excelFile = new FileInfo(Path.Combine(
            _cliOptions.Value.OutputPath, 
            string.Format(Constants.FileNames.DeploySheet, _cliOptions.Value.ApplicationName)));

        await excel.SaveAsAsync(excelFile, cancellationToken);
    }
}
