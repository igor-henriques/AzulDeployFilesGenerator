namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Labels
{
    public string App { get; init; }
    public string Tier { get; init; }
    public string Version { get; init; }
}
