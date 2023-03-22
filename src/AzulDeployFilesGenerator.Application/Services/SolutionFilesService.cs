namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    /// <summary>
    /// Searches for a file in a given directory and its subdirectories recursively. 
    /// Throws a <see cref="FileNotFoundException"/> if the file were not found.
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async ValueTask<string> GetFileContent(string relativePath, string fileName)
    {
        var filePath = Directory.GetFiles(relativePath, fileName, SearchOption.AllDirectories).FirstOrDefault();

        if (filePath is null)
        {
            throw new FileNotFoundException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, fileName));
        }

        return await File.ReadAllTextAsync(filePath);
    }
}
