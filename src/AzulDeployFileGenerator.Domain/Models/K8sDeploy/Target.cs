namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Target
{
    public int AverageUtilization { get; init; }
    public string Type { get; init; }
}
