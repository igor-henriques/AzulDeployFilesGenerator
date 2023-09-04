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
    public bool IsToGenerate { get; private set; }

    public CliFileGenerateModel SetIsToGenerate(bool isToGenerate)
    {
        IsToGenerate = isToGenerate;
        return this;
    }

    public bool RequiresDeployName()
    {
        return FileName is Constants.FileNames.K8sYaml 
            or Constants.FileNames.IsaBkoYaml 
            || FileName.Replace("{0}", string.Empty).Contains(".xlsx");
    }

    public static implicit operator CliFileGenerateModel(string fileName)
    {
        return new CliFileGenerateModel(fileName, true);
    }
}
