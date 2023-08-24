namespace AzulDeployFileGenerator.Domain.Models.Cli;

public sealed record CliCommandOptions
{
    public string SolutionPath { get; private set; }
    public string OutputPath { get; private set; }
    public string ApplicationType { get; private set; }
    public string ApplicationName { get; private set; }
    public string DeployName { get; private set; }

    public CliCommandOptions SetSolutionPath(string solutionPath)
    {
        SolutionPath = solutionPath;
        return this;
    }

    public CliCommandOptions SetOutputPath(string outputPath)
    {
        OutputPath = outputPath;
        return this;
    }

    public CliCommandOptions SetApplicationType(string applicationType)
    {
        ApplicationType = applicationType;
        return this;
    }

    public CliCommandOptions SetApplicationName(string applicationName)
    {
        ApplicationName = applicationName;
        return this;
    }

    public CliCommandOptions SetDeployName(string deployName)
    {
        DeployName = deployName;
        return this;
    }
}
