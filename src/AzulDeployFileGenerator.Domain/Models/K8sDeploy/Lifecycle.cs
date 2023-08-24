namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Lifecycle
{
    public PostStart PostStart { get; init; }
}
