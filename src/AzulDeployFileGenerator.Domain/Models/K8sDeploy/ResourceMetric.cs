namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ResourceMetric
{
    public string Name { get; init; }
    public Target Target { get; init; }
}
