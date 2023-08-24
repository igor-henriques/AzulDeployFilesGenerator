namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Exec
{
    public List<string> Command { get; init; }
}