namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ResourcesLimits
{
    public string Cpu { get; init; }
    public string Memory { get; init; }
}
