namespace AzulDeployFileGenerator.Domain.Models.Cli;

public sealed record CliFileGenerateModel
{
    public CliFileGenerateModel(string fileName, bool isToGenerate)
    {
        FileName = fileName;
        IsToGenerate = isToGenerate;
    }

    public CliFileGenerateModel()
    {

    }

    public string FileName { get; init; }
    public bool IsToGenerate { get; init; }

    public bool RequiresDeployName()
    {
        return FileName is Constants.FileNames.K8sYaml or Constants.FileNames.IsaBkoYaml;
    }
}
