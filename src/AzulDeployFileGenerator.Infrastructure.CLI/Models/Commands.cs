namespace AzulDeployFileGenerator.CLI.Models;

public sealed record Command
{
    public string Content { get; init; }
    public string Trigger { get; init; }
}