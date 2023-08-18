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
}
