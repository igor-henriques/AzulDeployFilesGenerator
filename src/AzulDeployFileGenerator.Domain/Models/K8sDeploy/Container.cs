namespace AzulDeployFileGenerator.Domain.Models.K8sDeploy;

public sealed record Container
{
    public string Name { get; init; }
    public string Image { get; init; }
    public List<EnvVariable> Env { get; init; }
    public List<Port> Ports { get; init; }
    public LivenessProbe LivenessProbe { get; init; }
    public Resources Resources { get; init; }
    public string TerminationMessagePath { get; init; }
    public string TerminationMessagePolicy { get; init; }
    public string ImagePullPolicy { get; init; }
    public Lifecycle Lifecycle { get; init; }
}
