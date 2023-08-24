namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record PostStart
{
    public Exec Exec { get; init; }
}
