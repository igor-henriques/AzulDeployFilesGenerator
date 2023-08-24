namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Template
{
    public Labels Labels { get; init; }
    public TemplateSpec Spec { get; init; }
    public Metadata Metadata { get; init; }
}
