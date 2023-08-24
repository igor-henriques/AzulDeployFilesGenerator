namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record LivenessProbe
{
    public HttpGet HttpGet { get; init; }
    public int InitialDelaySeconds { get; init; }
    public int PeriodSeconds { get; init; }
}
