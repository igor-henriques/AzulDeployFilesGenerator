namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Secret
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public Metadata Metadata { get; init; }
    public Dictionary<string, string> Data { get; init; }
    public string Type { get; init; }
}
