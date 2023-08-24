namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record HostAlias
{
    public string Ip { get; init; }
    public List<string> Hostnames { get; init; }
}
