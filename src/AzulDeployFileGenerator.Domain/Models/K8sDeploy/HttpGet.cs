namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record HttpGet
{
    public string Path { get; init; }
    public int Port { get; init; }
}
