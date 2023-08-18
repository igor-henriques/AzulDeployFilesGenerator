namespace AzulDeployFileGenerator.Domain.Models.Cli;

public sealed record CliCommandOptions
{
    public string SolutionPath { get; private set; }
    public string OutputPath { get; private set; }

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
}
