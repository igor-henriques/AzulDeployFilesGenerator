namespace AzulDeployFilesGenerator.Application.Factories;

internal sealed class DockerfileFactory : IDockerfileFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly IOptions<ApplicationDefaultsOptions> _appDefaultsOptions;

    public DockerfileFactory(
        ISolutionFilesService solutionFilesService,
        IOptions<CliCommandOptions> cliOptions,
        IOptions<ApplicationDefaultsOptions> appDefaultsOptions)
    {
        _solutionFilesService = solutionFilesService;
        _cliOptions = cliOptions;
        _appDefaultsOptions = appDefaultsOptions;
    }

    private Task<StringBuilder> GenerateDockerfileBuilder(CancellationToken cancellationToken = default)
    {
        _solutionFilesService.ValidateNugetConfig();

        StringBuilder builder = new();

        builder.AppendLine($"FROM {_appDefaultsOptions.Value.AzulAspNetSdkImage} AS base");
        builder.AppendLine("WORKDIR /app");

        builder.Append('\n');

        builder.AppendLine($"FROM {_appDefaultsOptions.Value.AzulDotNetSdkImage} AS build");
        builder.AppendLine("WORKDIR /src");

        builder.Append('\n');

        builder.AppendLine("COPY [\"*.sln\", \"./\"]");
        builder.AppendLine("COPY [\"./nuget.config\", \"./\"]");

        var assemblyFiles = _solutionFilesService.FindAllCsprojFiles();

        foreach (var assembly in assemblyFiles)
        {
            FileInfo fileInfo = new(assembly);
            var parentDirectoryName = fileInfo.DirectoryName[(fileInfo.DirectoryName.LastIndexOf('\\') + 1)..];
            var csprojName = fileInfo.Name;

            builder.AppendLine($"COPY [\"{parentDirectoryName}/{csprojName}\", \"{parentDirectoryName}/\"]");
        }

        (var parentDirectoryEntrypoint, var entrypointAssemblyName) = _solutionFilesService.FindEntrypointAssemblyAsync(cancellationToken: cancellationToken);

        builder.Append('\n');
        builder.AppendLine($"RUN dotnet restore \"{parentDirectoryEntrypoint}/{entrypointAssemblyName}\"");
        builder.AppendLine("COPY . .");
        builder.Append('\n');

        builder.AppendLine($"WORKDIR src/{parentDirectoryEntrypoint}");
        builder.AppendLine("RUN mv appsettings.Docker.json appsettings.json");

        if (_cliOptions.Value.ApplicationType is EApplicationType.Consumer)
        {
            builder.AppendLine($"RUN dotnet build \"{entrypointAssemblyName}\" -c Release -o /app/build");
        }

        builder.Append('\n');

        builder.AppendLine("FROM build AS publish");
        builder.AppendLine($"RUN dotnet publish \"{entrypointAssemblyName}\" -c Release -o /app/publish");

        builder.Append('\n');

        builder.AppendLine("FROM base AS final");
        builder.AppendLine("WORKDIR /app");
        builder.AppendLine("COPY --from=publish /app/publish .");

        builder.Append('\n');

        if (_solutionFilesService.FindAnySslCertificates(out List<string> sslCertificatesPaths))
        {
            foreach (var certificate in sslCertificatesPaths)
            {
                FileInfo certificateInfo = new(certificate);
                var certificateFileName = certificateInfo.Name;
                var certificateParentDirectory = certificateInfo.DirectoryName[(certificateInfo.DirectoryName.LastIndexOf('\\') + 1)..];

                builder.AppendLine($"RUN mv {certificateParentDirectory}/{certificateFileName} /usr/local/share/ca-certificates/{certificateFileName}.crt");
                builder.AppendLine($"RUN chmod 644 /usr/local/share/ca-certificates/{certificateFileName}.crt");
                builder.AppendLine("RUN update-ca-certificates");

                builder.Append('\n');
            }
        }

        if (_cliOptions.Value.ApplicationType is EApplicationType.Api)
        {
            builder.AppendLine("EXPOSE 80");
        }

        builder.AppendLine($"ENTRYPOINT [\"dotnet\", \"{_cliOptions.Value.ApplicationName}.dll\"]");

        return Task.FromResult(builder);
    }

    /// <summary>
    /// Generates Dockerfile for Azul environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> BuildAzulDockerfile(CancellationToken cancellationToken = default)
    {
        var dockerfileBuilder = await GenerateDockerfileBuilder(cancellationToken);
        return dockerfileBuilder.ToString();
    }

    /// <summary>
    /// Generates Dockerfile for Online environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> BuildOnlineDockerfile(CancellationToken cancellationToken = default)
    {
        var dockerfileBuilder = await GenerateDockerfileBuilder(cancellationToken);

        dockerfileBuilder.Replace(_appDefaultsOptions.Value.AzulDotNetSdkImage, _appDefaultsOptions.Value.OnlineDotNetSdkImage);
        dockerfileBuilder.Replace(_appDefaultsOptions.Value.AzulAspNetSdkImage, _appDefaultsOptions.Value.OnlineAspNetSdkImage);

        return dockerfileBuilder.ToString();
    }
}
