namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Metadata
{
    public string Name { get; init; }
    public string Namespace { get; init; }
    public Annotations Annotations { get; init; }
    public Labels Labels { get; init; }
}
