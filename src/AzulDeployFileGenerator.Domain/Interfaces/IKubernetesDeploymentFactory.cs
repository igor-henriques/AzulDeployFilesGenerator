namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IKubernetesDeploymentFactory
{
    /// <summary>
    /// Builds a Kubernetes deployment file specifically for the Azul environment.
    /// </summary>
    /// <param name="appSettings">Application settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Azul Kubernetes deployment file.</returns>
    Task<string> BuildAzulKubernetesDeployment(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a Kubernetes deployment file specifically for the Online environment.
    /// </summary>
    /// <param name="appSettings">Application settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the Online Kubernetes deployment file.</returns>
    Task<string> BuildOnlineKubernetesDeployment(AppSettings appSettings, CancellationToken cancellationToken = default);
}
