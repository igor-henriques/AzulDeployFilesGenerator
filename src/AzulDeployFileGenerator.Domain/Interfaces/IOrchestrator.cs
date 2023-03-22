namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IOrchestrator
{
    Task OrchestrateAsync(CancellationToken cancellationToken = default);
}
