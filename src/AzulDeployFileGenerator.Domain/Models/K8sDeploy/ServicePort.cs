namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ServicePort
{
    public int Port { get; init; }
    public int TargetPort { get; init; }
    public string Protocol { get; init; }
}
