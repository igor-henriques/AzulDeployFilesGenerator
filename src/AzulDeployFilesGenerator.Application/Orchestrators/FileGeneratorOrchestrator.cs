namespace AzulDeployFilesGenerator.Application.Orchestrators;

internal sealed class FileGeneratorOrchestrator : IOrchestrator
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly ILogger<FileGeneratorOrchestrator> _logger;

    public FileGeneratorOrchestrator(
        IOptions<CliCommandOptions> cliOptions,
        ISolutionFilesService solutionFilesService,
        ILogger<FileGeneratorOrchestrator> logger)
    {
        _cliOptions = cliOptions;
        this._solutionFilesService = solutionFilesService;
        _logger = logger;
    }

    public async Task OrchestrateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Looking for {fileName}", Constants.FileNames.AppSettings);
        var appsettings = await _solutionFilesService.GetFileContent(_cliOptions.Value.SolutionPath, Constants.FileNames.AppSettings);
        await Console.Out.WriteLineAsync(appsettings);
    }
}
