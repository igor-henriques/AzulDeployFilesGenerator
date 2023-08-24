namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record HorizontalPodAutoscalerSpec
{
    public string MinReplicas { get; init; }
    public string MaxReplicas { get; init; }
    public ScaleTargetRef ScaleTargetRef { get; init; }
    public List<Metric> Metrics { get; init; }
}
