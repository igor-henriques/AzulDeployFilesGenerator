namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Metric
{
    public string Type { get; init; }
    public ResourceMetric Resource { get; init; }
}
