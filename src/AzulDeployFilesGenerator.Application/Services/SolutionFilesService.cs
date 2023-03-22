namespace AzulDeployFilesGenerator.Application.Services;

internal sealed class SolutionFilesService : ISolutionFilesService
{
    public async ValueTask<string> GetFileContent(string relativePath, string fileName)
    {
        foreach (string file in Directory.GetFiles(relativePath, fileName))
        {
            return await File.ReadAllTextAsync(file);
        }

        foreach (string subdirectory in Directory.GetDirectories(relativePath))
        {
            await GetFileContent(subdirectory, fileName);
        }

        throw new ApplicationException(string.Format(Constants.Messages.FILE_NOT_FOUND_ERROR_MESSAGE, fileName));
    }
}
