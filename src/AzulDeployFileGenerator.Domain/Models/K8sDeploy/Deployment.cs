namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Deployment
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public Metadata Metadata { get; init; }
    public DeploymentSpec Spec { get; init; }
}
