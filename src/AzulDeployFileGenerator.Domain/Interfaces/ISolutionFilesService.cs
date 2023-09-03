namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ISolutionFilesService
{
    /// <summary>
    /// Searches for a file in a given directory and its subdirectories recursively. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    ValueTask<string> GetFileContentAsync(string fileName, string relativePath = null, CancellationToken cancellationToken = default);

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
    Task<bool> ContainsTextInAnyCSharpFileAsync(string text, string relativePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for a file *.sln in a given directory and its subdirectories. 
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">Throws if the *.sln file were not found</exception>
    /// <exception cref="InvalidOperationException">Throws if there's *.sln duplicates</exception>
    string GetSolutionName(string relativePath = null);

    /// <summary>
    /// Executes 'dotnet clean' command over the solution to remove all unnecessary files.
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// </summary>
    void CleanSolutionFiles(string relativePath = null);

    /// <summary>
    /// Searches all directories for the pattern *.cer files
    /// </summary>
    /// <param name="sslPath">List of *.cer files fullnames</param>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <returns>true if any certificates were found</returns>
    bool FindAnySslCertificates(out List<string> sslPath, string relativePath = null);

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
    Task ValidateNugetConfig(string relativePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for the entrypoint assembly. When this method finds the Program.cs class, it'll return the relative assembly name, along with its parent directory.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="cancellationToken"></param>    
    /// <returns>Parent Directory, *.csproj Name</returns>
    (string, string) FindEntrypointAssemblyAsync(string relativePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for all *.csproj files in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <returns></returns>
    string[] FindAllCsprojFiles(string relativePath = null);

    /// <summary>
    /// Search for a Subscribers in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<bool> HasAnySubscribers(string relativePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for a Publishers in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<bool> HasAnyPublishers(string relativePath = null, CancellationToken cancellationToken = default);

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
    Task<List<string>> GetCSharpFileWhereContainsText(string text, string relativePath = null, CancellationToken cancellationToken = default);
}
