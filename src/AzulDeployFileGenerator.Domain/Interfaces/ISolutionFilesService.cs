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
    /// Search for a specific string content in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="text"></param>
    /// <returns></returns>
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
    /// Validates if the Nuget.Config file exists in the solution path
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    void ValidateNugetConfig(string relativePath = null);

    /// <summary>
    /// Searches for the entrypoint assembly. When this method finds the Program.cs class, it'll return the relative assembly name, along with its parent directory.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <param name="cancellationToken"></param>    
    /// <returns>Parent Directory, *.csproj Name</returns>
    Task<(string, string)> FindEntrypointAssemblyAsync(string relativePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for all *.csproj files in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath">Defaults to CliCommandOptions.SolutionPath</param>
    /// <returns></returns>
    string[] FindAllCsprojFiles(string relativePath = null);
}
