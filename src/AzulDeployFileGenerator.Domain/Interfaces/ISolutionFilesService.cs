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
    ValueTask<string> GetFileContent(string relativePath, string fileName);
}
