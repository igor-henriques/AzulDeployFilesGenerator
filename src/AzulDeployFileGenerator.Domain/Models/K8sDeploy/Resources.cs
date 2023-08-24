namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Resources
{
    public ResourcesRequests Requests { get; init; }
    public ResourcesLimits Limits { get; init; }
}
