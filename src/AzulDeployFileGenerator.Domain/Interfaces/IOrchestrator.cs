namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IOrchestrator
{
    /// <summary>
    /// Starts the application
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task OrchestrateAsync(CancellationToken cancellationToken = default);
}
