namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record RollingUpdate
{
    public string MaxUnavailable { get; init; }
    public string MaxSurge { get; init; }
}
