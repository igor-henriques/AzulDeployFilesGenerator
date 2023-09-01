namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IKubernetesDeploymentFactory
{
    Task<string> BuildAzulKubernetesDeployment(AppSettings appSettings, CancellationToken cancellationToken = default);
    Task<string> BuildOnlineKubernetesDeployment(AppSettings appSettings, CancellationToken cancellationToken = default);
}
