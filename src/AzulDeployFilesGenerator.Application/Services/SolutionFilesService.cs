namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    private const string CSHARP_CLASS_EXTENSION = "*.cs";
    private const string CSHARP_ASSEMBLY_EXTENSION = "*.csproj";
    private const string CSHARP_PROJECT_EXTENSION = "*.sln";
    private const string SSL_CERTIFICATE_EXTENSION = "*.cer";

    private readonly IOptions<CliCommandOptions> _cliOptions;

    public SolutionFilesService(IOptions<CliCommandOptions> cliOptions)
    {
        _cliOptions = cliOptions;
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
    /// Search for a specific string content in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
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
    public async Task<(string, string)> FindEntrypointAssemblyAsync(string relativePath = null, CancellationToken cancellationToken = default)
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

        if (!(await File.ReadAllTextAsync(entrypointAssemblyPath.Single(), cancellationToken)).Contains("<AssemblyName>"))
        {
            throw new ApplicationException(Constants.Messages.NO_ASSEMBLY_NAME_DEFINED_ERROR_MESSAGE);
        }

        return (entrypointParentDirectory, new FileInfo(entrypointAssemblyPath.Single()).Name);
    }

    /// <summary>
    /// Validates if the Nuget.Config file exists in the solution path
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    public void ValidateNugetConfig(string relativePath = null)
    {
        var nugetConfigExists = Directory.GetFiles(relativePath ?? _cliOptions.Value.SolutionPath, Constants.FileNames.NugetConfig, SearchOption.AllDirectories).Any();
        if (!nugetConfigExists)
        {
            throw new ApplicationException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, Constants.FileNames.NugetConfig));
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
}
