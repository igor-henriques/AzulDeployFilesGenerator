namespace AzulDeployFileGenerator.Infrastructure.CLI.Models;

public sealed record CliCommandOptions
{
    public string SolutionPath { get; private set; }
    public string OutputPath { get; private set; }

    public CliCommandOptions SetSolutionPath(string solutionPath)
    {
        SolutionPath = solutionPath;
        return this;
    }

    //set output path
    public CliCommandOptions SetOutputPath(string outputPath)
    {
        OutputPath = outputPath;
        return this;
    }
}
