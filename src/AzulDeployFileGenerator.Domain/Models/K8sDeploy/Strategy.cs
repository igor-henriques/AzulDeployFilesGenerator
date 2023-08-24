namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Strategy
{
    public string Type { get; init; }
    public RollingUpdate RollingUpdate { get; init; }
}
