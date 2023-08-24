namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record HorizontalPodAutoscaler
{
    public string ApiVersion { get; init; }
    public string Kind { get; init; }
    public Metadata Metadata { get; init; }
    public HorizontalPodAutoscalerSpec Spec { get; init; }
}
