using AzulDeployFileGenerator.Domain.Models.Cli;

namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ICliService
{
    /// <summary>
    /// Interacts with the user via command line input to get the files to generate.
    /// </summary>
    /// <returns></returns>
    List<CliFileGenerateModel> GetRequestedFilesToGenerate();
}
