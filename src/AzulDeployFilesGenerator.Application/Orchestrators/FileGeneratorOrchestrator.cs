namespace AzulDeployFilesGenerator.Application.Orchestrators;

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

        var requestedFiles = _cliService.GetRequestedFilesToGenerate();
        foreach (var fileToGenerate in requestedFiles)
        {
            var task = fileToGenerate.FileName switch
            {
                Constants.FileNames.AppSettingsOnline => _solutionFilesService.GenerateAppSettingsOnline(appsettingsObj, cancellationToken),
                Constants.FileNames.AppSettingsDocker => _solutionFilesService.GenerateAppSettingsDocker(appsettingsObj, cancellationToken),
                _ => Task.CompletedTask
            };

            await task;
        }
    }
}
