using AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;
using AzulDeployFileGenerator.Domain.Models.Cli;

namespace AzulDeployFilesGenerator.Infrastructure.IoC.Container;

public static class ConfigureContainer
{
    public static IHostBuilder ConfigureDependencies(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.InjectServices();
        });

        return hostBuilder;
    }

    /// <summary>
    /// Inject <see cref="CliCommandOptions"/> into the <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="commands"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException">Thrown when insufficient CliCommands were provided.</exception>
    public static IHostBuilder ConfigureCliCommandOptions(this IHostBuilder hostBuilder, IEnumerable<CliCommand> commands)
    {
        var solutionPath = commands.FirstOrDefault(command => command.IsSolutionPathCommandType)?.Content;
        var outputPath = commands.FirstOrDefault(command => command.IsOutputPathCommandType)?.Content;

        if (string.IsNullOrEmpty(solutionPath) || string.IsNullOrEmpty(outputPath))
        {
            throw new ApplicationException(Constants.Messages.INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE);
        }

        hostBuilder.ConfigureServices(services => services.Configure<CliCommandOptions>(options =>
        {
            options.SetSolutionPath(solutionPath)
                   .SetOutputPath(outputPath);
        }));

        return hostBuilder;
    }

    private static void InjectServices(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddOptions();
        services.Configure<ConsoleLifetimeOptions>(options =>
        {
            options.SuppressStatusMessages = true;
        });
        services.AddSingleton<IOrchestrator, FileGeneratorOrchestrator>();
        services.AddSingleton<ISolutionFilesService, SolutionFilesService>();
        services.AddSingleton<ICliService, CliService>();
        services.AddTransient<IValidator<AppSettings>, AppSettingsValidator>();
    }
}
