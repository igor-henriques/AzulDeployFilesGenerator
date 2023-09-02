namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ITokenizedAppSettingsFactory
{
    string BuildTokenizedAppSettingsAsync(AppSettings appSettings, CancellationToken cancellationToken = default);
}
