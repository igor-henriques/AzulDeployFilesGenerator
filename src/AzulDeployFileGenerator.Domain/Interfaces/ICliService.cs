namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ICliService
{
    /// <summary>
    /// Interacts with the user via command line input to get the files to generate.
    /// </summary>    
    List<CliFileGenerateModel> GetRequestedFilesToGenerate();

    /// <summary>
    /// Interacts with the user via command line input to get the deploy name.
    /// </summary>    
    string GetDeployName();
}
