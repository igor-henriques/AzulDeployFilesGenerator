namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record EnvVariable
{
    public string Name { get; init; }
    public string Value { get; init; }
}
