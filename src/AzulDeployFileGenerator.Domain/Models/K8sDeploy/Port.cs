namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Port
{
    public string Name { get; init; }
    public int ContainerPort { get; init; }
    public string Protocol { get; init; }
}
