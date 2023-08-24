namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record TemplateSpec
{
    public List<HostAlias> HostAliases { get; init; }
    public List<Container> Containers { get; init; }
    public string RestartPolicy { get; init; }
}
