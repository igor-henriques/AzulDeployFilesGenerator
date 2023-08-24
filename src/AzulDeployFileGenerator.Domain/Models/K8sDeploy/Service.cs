namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Service
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public Metadata Metadata { get; init; }
    public ServiceSpec Spec { get; init; }
}