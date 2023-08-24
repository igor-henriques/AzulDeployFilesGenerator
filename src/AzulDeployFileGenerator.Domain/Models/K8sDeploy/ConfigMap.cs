namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ConfigMap
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public Metadata Metadata { get; init; }
    public Dictionary<string, string> Data { get; init; }
}
