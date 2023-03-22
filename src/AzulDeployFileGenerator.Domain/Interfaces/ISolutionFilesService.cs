namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ISolutionFilesService
{
    ValueTask<string> GetFileContent(string relativePath, string fileName);
}
