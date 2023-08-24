namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Selector
{
    public Labels MatchLabels { get; init; }
}
