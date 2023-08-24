namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record DeploymentSpec
{
    public Selector Selector { get; init; }
    public int Replicas { get; init; }
    public Template Template { get; init; }
    public Strategy Strategy { get; init; }
}
