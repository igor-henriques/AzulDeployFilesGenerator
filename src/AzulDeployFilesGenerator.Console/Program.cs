ASCIIArt.PrintWelcome();

var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
}).CreateLogger(nameof(AzulDeployFilesGenerator));

var cancellationTokenSource = new CancellationTokenSource();

try
{
    if (args.Any(arg => arg.Contains(CliCommand.HELP_COMMAND_ID)))
    {
        Console.WriteLine(CliCommand.GetHelpCommands());
        Environment.Exit(0);
    }

    var commands = CliParser.ParseArgsAsCommands(args);

    var host = Host.CreateDefaultBuilder()
        .ConfigureDependencies()
        .ConfigureCliCommandOptions(commands)
        .Build();    

    await host.Services
        .GetRequiredService<IOrchestrator>()
        .OrchestrateAsync(cancellationTokenSource.Token);
}
catch (Exception ex)
{    
    logger.LogError(Constants.Messages.GLOBAL_EXCEPTION_HANDLER_ERROR_MESSAGE, ex.Message);
    Console.ReadKey();
    Environment.Exit(Environment.ExitCode);
}
finally
{
    cancellationTokenSource.Cancel();
}