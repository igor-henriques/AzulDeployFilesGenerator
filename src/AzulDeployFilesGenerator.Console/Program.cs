ASCIIArt.PrintWelcome();

var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
}).CreateLogger(nameof(AzulDeployFilesGenerator));

try
{
    var commands = CliParser.ParseArgsAsCommands(args);

    var host = Host.CreateDefaultBuilder()
        .ConfigureDependencies()
        .ConfigureCliCommandOptions(commands)
        .Build();

    await host.Services
        .GetRequiredService<IOrchestrator>()
        .OrchestrateAsync(CancellationToken.None);
}
catch (Exception ex)
{
    logger.LogError(Constants.Messages.GLOBAL_EXCEPTION_HANDLER_ERROR_MESSAGE, ex.Message);
    Console.ReadKey();
    Environment.Exit(Environment.ExitCode);
}