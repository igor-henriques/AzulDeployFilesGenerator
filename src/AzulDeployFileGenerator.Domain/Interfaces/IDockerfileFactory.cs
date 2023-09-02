namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IDockerfileFactory
{
    /// <summary>
    /// Generates Dockerfile for Azul environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> BuildAzulDockerfile(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates Dockerfile for Online environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> BuildOnlineDockerfile(CancellationToken cancellationToken = default);
}
