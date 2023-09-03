using AzulDeployFileGenerator.Domain.Models.Options;

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

    IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<ApplicationDefaultsOptions>()
            .Build();

    var commands = CliParser.ParseArgsAsCommands(args);

    var host = Host.CreateDefaultBuilder()
        .ConfigureDependencies(configuration)
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