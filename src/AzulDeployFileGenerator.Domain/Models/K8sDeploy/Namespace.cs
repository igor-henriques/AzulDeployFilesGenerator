namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Namespace
{
    public string ApiVersion { get; init; } = "v1";
    public string Kind { get; init; } = "Namespace";
    public Metadata Metadata { get; init; }
}
