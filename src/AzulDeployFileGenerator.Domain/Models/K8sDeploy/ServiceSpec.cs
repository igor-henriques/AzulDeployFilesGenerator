namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record ServiceSpec
{
    public List<ServicePort> Ports { get; init; }
    public Selector Selector { get; init; }
}
