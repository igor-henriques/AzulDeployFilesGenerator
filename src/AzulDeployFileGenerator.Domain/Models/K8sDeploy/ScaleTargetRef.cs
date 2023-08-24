namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ScaleTargetRef
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public string Name { get; init; }
}
