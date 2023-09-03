namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    private const string CSHARP_CLASS_EXTENSION = "*.cs";
    private const string CSHARP_ASSEMBLY_EXTENSION = "*.csproj";
    private const string CSHARP_PROJECT_EXTENSION = "*.sln";
    private const string SSL_CERTIFICATE_EXTENSION = "*.cer";

    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IOptions<ApplicationDefaultsOptions> _appDefaultsOptions;

    public SolutionFilesService(IOptions<CliCommandOptions> cliOptions, IOptions<ApplicationDefaultsOptions> appDefaultsOptions)
    {
        _cliOptions = cliOptions;
        _appDefaultsOptions = appDefaultsOptions;
    }

    public string[] FindAllCsprojFiles(string relativePath = null)
    {
        return Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, CSHARP_ASSEMBLY_EXTENSION, SearchOption.AllDirectories);
    }

    /// <summary>
    /// Executes 'dotnet clean' command over the solution to remove all unnecessary files.
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// </summary>
    public void CleanSolutionFiles(string relativePath = null)
    {
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = "clean",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = relativePath ?? _cliOptions.Value.SolutionPath
        };

        process.StartInfo = startInfo;
        process.Start();
    }

    /// <summary>
    /// Searches for a file in a given directory and its subdirectories recursively. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async ValueTask<string> GetFileContentAsync(string fileName, string relativePath = null, CancellationToken cancellationToken = default)
    {
        var filePath = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, fileName, SearchOption.AllDirectories).FirstOrDefault();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, fileName));
        }

        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    /// <summary>
    /// Searches for a file *.sln in a given directory and its subdirectories. 
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">Throws if the *.sln file were not found</exception>
    /// <exception cref="InvalidOperationException">Throws if there's *.sln duplicates</exception>
    public string GetSolutionName(string relativePath = null)
    {
        var solutionFiles = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, CSHARP_PROJECT_EXTENSION, SearchOption.AllDirectories);

        if (solutionFiles.Length > 1)
        {
            throw new InvalidOperationException($".sln duplicates found in the solution path provided. Fix it and try again.");
        }

        var solutionFile = solutionFiles.FirstOrDefault();

        if (string.IsNullOrEmpty(solutionFile) || !File.Exists(solutionFile))
        {
            throw new FileNotFoundException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, ".sln"));
        }

        FileInfo fileInfo = new(solutionFile);

        return fileInfo.Name.Replace(".sln", string.Empty);
    }

    /// <summary>
    /// Checks if the specified text is present in any C# file within a directory.
    /// </summary>
    /// <param name="text">The text to search for in the C# files.</param>
    /// <param name="relativePath">The relative path where to look for the C# files. Defaults to the solution path.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission.</exception>
    /// <exception cref="ArgumentException">Thrown when one of the arguments provided to the method is invalid.</exception>
    /// <returns>A <see cref="Task"/> that returns true if the text is found, otherwise false.</returns>
    public async Task<bool> ContainsTextInAnyCSharpFileAsync(string text, string relativePath = null, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, CSHARP_CLASS_EXTENSION, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var classRawText = await File.ReadAllTextAsync(file, cancellationToken);

            if (classRawText.Contains(text))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches for the entrypoint assembly. When this method finds the Program.cs class, it'll return the relative assembly name, along with its parent directory.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="cancellationToken"></param>    
    /// <returns>Parent Directory, *.csproj Name</returns>
    public (string, string) FindEntrypointAssemblyAsync(string relativePath = null, CancellationToken cancellationToken = default)
    {
        var programFiles = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, Constants.FileNames.Program, SearchOption.AllDirectories);

        if (programFiles.Length > 1)
        {
            throw new ApplicationException(string.Format(Constants.Messages.MORE_THAN_ONE_FILE_FOUND_ERROR_MESSAGE, Constants.FileNames.Program));
        }

        var programFile = programFiles.Single();
        FileInfo entrypointFile = new(programFile);
        var entrypointAssemblyPath = Directory.GetFiles(entrypointFile.DirectoryName, CSHARP_ASSEMBLY_EXTENSION, SearchOption.TopDirectoryOnly);

        if (entrypointAssemblyPath.Length > 1)
        {
            throw new ApplicationException(Constants.Messages.MORE_THAN_ONE_ASSEMBLY_FOUND_ERROR_MESSAGE);
        }

        var entrypointParentDirectory = entrypointFile.DirectoryName[(entrypointFile.DirectoryName.LastIndexOf('\\') + 1)..];

        return (entrypointParentDirectory, new FileInfo(entrypointAssemblyPath.Single()).Name);
    }

    /// <summary>
    /// Validates the NuGet configuration file in the given or default directory.
    /// </summary>
    /// <param name="relativePath">The relative path where to look for the NuGet config file. Defaults to the solution path.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="ApplicationException">Thrown when no NuGet config files are found in the directory.</exception>
    /// <exception cref="ApplicationException">Thrown when more than one NuGet config file is found in the directory.</exception>
    /// <exception cref="ApplicationException">Thrown when the Azul Framework NuGet key is not defined in the options.</exception>
    /// <exception cref="ApplicationException">Thrown when the Azul Framework NuGet key is not found in the NuGet config file.</exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous validation operation.</returns>
    public async Task ValidateNugetConfig(string relativePath = null, CancellationToken cancellationToken = default)
    {
        var nugetConfigs = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, Constants.FileNames.NugetConfig, SearchOption.AllDirectories);

        if (!nugetConfigs.Any())
        {
            throw new ApplicationException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, Constants.FileNames.NugetConfig));
        }

        if (nugetConfigs.Length > 1)
        {
            throw new ApplicationException(string.Format(Constants.Messages.MORE_THAN_ONE_FILE_FOUND_ERROR_MESSAGE, Constants.FileNames.NugetConfig));
        }

        var nugetConfig = nugetConfigs.First();
        var nugetContent = await File.ReadAllTextAsync(nugetConfig, cancellationToken);

        if (string.IsNullOrWhiteSpace(_appDefaultsOptions.Value.AzulFrameworkNugetKey))
        {
            throw new ApplicationException(Constants.Messages.NUGET_CONFIG_KEY_NOT_DEFINED_ERROR_MESSAGE);
        }

        if (!nugetContent.Contains(_appDefaultsOptions.Value.AzulFrameworkNugetKey))
        {
            throw new ApplicationException(Constants.Messages.NUGET_CONFIG_KEY_NOT_FOUND_ERROR_MESSAGE);
        }
    }

    /// <summary>
    /// Searches all directories for the pattern *.cer files
    /// </summary>
    /// <param name="sslPath">List of *.cer files fullnames</param>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <returns>true if any certificates were found</returns>
    public bool FindAnySslCertificates(out List<string> sslPath, string relativePath = null)
    {
        sslPath = new List<string>();

        var certificates = Directory.GetFiles(relativePath ??= _cliOptions.Value.SolutionPath, SSL_CERTIFICATE_EXTENSION, SearchOption.AllDirectories)
            .Where(path => !path.Contains("Debug")); //Need to ignore "Debug" folder and contents case the application is being executed on VS

        var hasAnyCertificates = certificates.Any();
        if (hasAnyCertificates)
        {
            sslPath.AddRange(certificates);
        }

        return hasAnyCertificates;
    }

    /// <summary>
    /// Search for Subscribers in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<bool> HasAnySubscribers(string relativePath = null, CancellationToken cancellationToken = default)
    {
        return await ContainsTextInAnyCSharpFileAsync(Constants.SUBSCRIBER_FILE_IDENTIFIER, relativePath, cancellationToken);
    }

    /// <summary>
    /// Search for Publishers in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<bool> HasAnyPublishers(string relativePath = null, CancellationToken cancellationToken = default)
    {
        return await ContainsTextInAnyCSharpFileAsync(Constants.PUBLISHER_FILE_IDENTIFIER, relativePath, cancellationToken);
    }

    /// <summary>
    /// Retrieves a list of C# files from the specified directory that contain the given text.
    /// </summary>
    /// <param name="text">The text to search for in the C# files.</param>
    /// <param name="relativePath">The relative path where to look for the C# files. Defaults to the solution path.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission.</exception>
    /// <exception cref="ArgumentException">Thrown when one of the arguments provided to the method is invalid.</exception>
    /// <returns>A <see cref="Task"/> that returns a list of C# files containing the specified text.</returns>
    public async Task<List<string>> GetCSharpFileWhereContainsText(string text, string relativePath = null, CancellationToken cancellationToken = default)
    {
        List<string> csharpFiles = new();

        var files = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, CSHARP_CLASS_EXTENSION, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var classRawText = await File.ReadAllTextAsync(file, cancellationToken);

            if (classRawText.Contains(text))
            {
                csharpFiles.Add(classRawText);
            }
        }

        return csharpFiles;
    }
}
