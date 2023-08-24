namespace AzulDeployFilesGenerator.Application.Orchestrators;

/// <summary>
/// Here is where the actual magic is orchestrated. The entry point.
/// </summary>
internal sealed class FileGeneratorOrchestrator : IOrchestrator
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly ILogger<FileGeneratorOrchestrator> _logger;
    private readonly IValidator<AppSettings> _appSettingsValidator;
    private readonly ICliService _cliService;

    public FileGeneratorOrchestrator(
        IOptions<CliCommandOptions> cliOptions,
        ISolutionFilesService solutionFilesService,
        ILogger<FileGeneratorOrchestrator> logger,
        IValidator<AppSettings> appSettingsValidator,
        ICliService cliService)
    {
        _cliOptions = cliOptions;
        _solutionFilesService = solutionFilesService;
        _logger = logger;
        _appSettingsValidator = appSettingsValidator;
        _cliService = cliService;
    }

    public async Task OrchestrateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching for {fileName}", Constants.FileNames.AppSettings);

        var appsettingsString = await _solutionFilesService.GetFileContent(
            _cliOptions.Value.SolutionPath,
            Constants.FileNames.AppSettings,
            cancellationToken);

        _logger.LogInformation("{fileName} found", Constants.FileNames.AppSettings);
        
        var appsettingsObj = JsonConvert.DeserializeObject<AppSettings>(appsettingsString);
        await _appSettingsValidator.ValidateAndThrowAsync(appsettingsObj, cancellationToken);

        _cliOptions.Value.SetApplicationName(
            _solutionFilesService.GetSolutionName(_cliOptions.Value.SolutionPath));

        var requestedFiles = _cliService.GetRequestedFilesToGenerate();

        foreach (var fileToGenerate in requestedFiles)
        {
            try
            {
                if (fileToGenerate.RequiresDeployName() && string.IsNullOrWhiteSpace(_cliOptions.Value.DeployName))
                {
                    _cliOptions.Value.SetDeployName(_cliService.GetDeployName());
                }

                var task = fileToGenerate.FileName switch
                {
                    Constants.FileNames.AppSettingsOnline => _solutionFilesService.GenerateAppSettingsOnline(appsettingsObj, cancellationToken),
                    Constants.FileNames.AppSettingsDocker => _solutionFilesService.GenerateAppSettingsDocker(appsettingsObj, cancellationToken),
                    Constants.FileNames.K8sYaml => _solutionFilesService.GenerateK8sDeploy(appsettingsObj, cancellationToken),
                    _ => throw new NotImplementedException("Feature not implemented yet.")
                };

                await task;
            }
            catch (Exception ex)
            {
                if (requestedFiles.Count > 1)
                {
                    _logger.LogError("Error generating {fileName}. Exception: {exception}\nMoving to the next.", 
                        fileToGenerate.FileName,
                        ex);
                }
                else
                {
                    _logger.LogError("Error generating {fileName}. Exception: {exception}\nNo more files to generate.",
                        fileToGenerate.FileName,
                        ex);
                }

                continue;
            }

            _logger.LogInformation("{fileName} successfully generated at {outputPath}", 
                fileToGenerate.FileName,
                _cliOptions.Value.OutputPath);
        }
    }
}
