namespace AzulDeployFilesGenerator.Application.Orchestrators;

/// <summary>
/// Here is where the actual magic is orchestrated. The entry point.
/// </summary>
internal sealed class FileGeneratorOrchestrator : IOrchestrator
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly IDeployFileGeneratorService _deployFileGeneratorService;
    private readonly ILogger<FileGeneratorOrchestrator> _logger;
    private readonly IValidator<AppSettings> _appSettingsValidator;
    private readonly ICliService _cliService;

    public FileGeneratorOrchestrator(
        IOptions<CliCommandOptions> cliOptions,
        ISolutionFilesService solutionFilesService,
        ILogger<FileGeneratorOrchestrator> logger,
        IValidator<AppSettings> appSettingsValidator,
        ICliService cliService,
        IDeployFileGeneratorService deployFileGeneratorService)
    {
        _cliOptions = cliOptions;
        _solutionFilesService = solutionFilesService;
        _logger = logger;
        _appSettingsValidator = appSettingsValidator;
        _cliService = cliService;
        _deployFileGeneratorService = deployFileGeneratorService;
    }

    public async Task OrchestrateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(Constants.Messages.EXECUTING_DOTNET_CLEAN_MESSAGE);

        _solutionFilesService.CleanSolutionFiles();

        _logger.LogInformation(Constants.Messages.SOLUTION_CLEANED);

        _logger.LogInformation("Searching for {fileName}\n", Constants.FileNames.AppSettings);

        var appsettingsString = await _solutionFilesService.GetFileContentAsync(
            Constants.FileNames.AppSettings,
            cancellationToken: cancellationToken);

        _logger.LogInformation("{fileName} found\n", Constants.FileNames.AppSettings);
        
        var appsettingsObj = JsonConvert.DeserializeObject<AppSettings>(appsettingsString);
        await _appSettingsValidator.ValidateAndThrowAsync(appsettingsObj, cancellationToken);

        _cliOptions.Value.SetApplicationName(
            _solutionFilesService.GetSolutionName());

        var requestedFiles = _cliService.GetRequestedFilesToGenerate();

        if (!requestedFiles.Any())
        {
            _logger.LogInformation(Constants.Messages.NO_ACTIONS_MESSAGE);
            Environment.Exit(0);
        }

        foreach (var fileToGenerate in requestedFiles)
        {
            try
            {
                if (fileToGenerate.RequiresDeployName() && string.IsNullOrWhiteSpace(_cliOptions.Value.DeployName))
                {
                    _cliOptions.Value.SetDeployName(_cliService.GetDeployName());
                    _cliOptions.Value.SetImageName(_cliService.GetImageName());
                }

                var task = string.Format(fileToGenerate.FileName, _cliOptions.Value.ApplicationName) switch
                {
                    Constants.FileNames.AppSettingsOnline => _deployFileGeneratorService.GenerateAppSettingsOnline(appsettingsObj, cancellationToken),
                    Constants.FileNames.AppSettingsDocker => _deployFileGeneratorService.GenerateAppSettingsDocker(appsettingsObj, cancellationToken),
                    Constants.FileNames.K8sYaml => _deployFileGeneratorService.GenerateAzulK8sDeploy(appsettingsObj, cancellationToken),
                    Constants.FileNames.IsaBkoYaml => _deployFileGeneratorService.GenerateOnlineK8sDeploy(appsettingsObj, cancellationToken),
                    Constants.FileNames.Dockerfile => _deployFileGeneratorService.GenerateAzulDockerfile(cancellationToken),
                    Constants.FileNames.DockerfileOnline => _deployFileGeneratorService.GenerateOnlineDockerfile(cancellationToken),
                    var fileName when fileName == string.Format(Constants.FileNames.DeploySheet, _cliOptions.Value.ApplicationName) => _deployFileGeneratorService.GenerateTokenizationExcelSheet(appsettingsObj, cancellationToken),
                    _ => throw new ApplicationException(string.Format(Constants.Messages.INVALID_FILE_NAME_ERROR_MESSAGE, fileToGenerate.FileName))
                };

                await task;
            }
            catch (Exception ex)
            {
                var errorMessage = requestedFiles.Count > 1
                    ? "Error generating {fileName}. Exception: {exception}\nMoving to the next."
                    : "Error generating {fileName}. Exception: {exception}\nNo more files to generate.";

                _logger.LogError(errorMessage,
                        fileToGenerate.FileName,
                        ex);

                //If we had an exception generating some file, we want to continue so we don't have a success log.
                continue;
            }

            _logger.LogInformation("'{fileName}' successfully generated at {outputPath}\n\n", 
                string.Format(fileToGenerate.FileName, _cliOptions.Value.ApplicationName),
                _cliOptions.Value.OutputPath);
        }
    }
}
