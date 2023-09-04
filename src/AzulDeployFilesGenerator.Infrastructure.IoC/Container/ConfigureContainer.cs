namespace AzulDeployFilesGenerator.Infrastructure.IoC.Container;

public static class ConfigureContainer
{
    public static IHostBuilder ConfigureDependencies(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.InjectServices(configuration);
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
        var solutionPath = commands.FirstOrDefault(command => CliCommand.IsSolutionPathCommandType(command.Trigger))?.Content;
        var outputPath = commands.FirstOrDefault(command => CliCommand.IsOutputPathCommandType(command.Trigger))?.Content;
        var appType = commands.FirstOrDefault(command => CliCommand.IsAppTypeCommandType(command.Trigger))?.Content;
        var deployName = commands.FirstOrDefault(command => CliCommand.IsDeployNameCommandType(command.Trigger))?.Content;
        var imageName = commands.FirstOrDefault(command => CliCommand.IsImageNameCommandType(command.Trigger))?.Content;
        var generateAllFiles = commands.Any(command => CliCommand.IsGenerateAllFilesCommandType(command.Trigger));        

        hostBuilder.ConfigureServices(services => services.Configure<CliCommandOptions>(options =>
        {
            if (!string.IsNullOrWhiteSpace(solutionPath))
            {
                options.SetSolutionPath(solutionPath);
            }

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                options.SetOutputPath(outputPath);
            }

            if (!string.IsNullOrWhiteSpace(appType))
            {
                options.SetApplicationType(appType);
            }

            if (!string.IsNullOrWhiteSpace(deployName))
            {
                options.SetDeployName(deployName);
            }

            if (!string.IsNullOrWhiteSpace(imageName))
            {
                options.SetImageName(imageName);
            }

            options.SetGenerateAllFiles(generateAllFiles);
        }));

        return hostBuilder;
    }

    private static void InjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();

        services.Configure<ConsoleLifetimeOptions>(options =>
        {
            options.SuppressStatusMessages = true;
        });

        services.Configure<ApplicationDefaultsOptions>(configuration.GetSection("ApplicationDefaultsOptions"));

        services.AddOptions();

        services.AddSingleton(_ => configuration);
        services.AddSingleton<IOrchestrator, FileGeneratorOrchestrator>();
        services.AddSingleton<IDeployFileGeneratorService, DeployFileGeneratorService>();
        services.AddSingleton<ISolutionFilesService, SolutionFilesService>();
        services.AddSingleton<ICliService, CliService>();

        //We don't want to share any validation result between the different instances of the validator injected by the IoC
        services.AddTransient<IValidator<AppSettings>, AppSettingsValidator>();

        services.AddSingleton<IKubernetesDeploymentFactory, KubernetesDeploymentFactory>();
        services.AddSingleton<IExcelSheetFactory, ExcelSheetFactory>();
        services.AddSingleton<ITokenizedAppSettingsFactory, TokenizedAppSettingsFactory>();
        services.AddSingleton<IDockerfileFactory, DockerfileFactory>();
    }
}
