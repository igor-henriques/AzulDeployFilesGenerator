namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ISolutionFilesService
{
    /// <summary>
    /// Searches for a file in a given directory and its subdirectories recursively. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    ValueTask<string> GetFileContent(string relativePath, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for a specific string content in all C# classes (.cs) in a given directory and its subdirectories.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<bool> AnySolutionClassContainsText(string relativePath, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the appsettings.json file for the online environment.
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAppSettingsOnline(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a tokenized version of the appsettings, stands for appsettings.Docker.json
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAppSettingsDocker(AppSettings appSettings, CancellationToken cancellationToken = default);
}
