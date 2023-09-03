namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface ITokenizedAppSettingsFactory
{
    /// <summary>
    /// Builds a string representation of tokenized AppSettings.
    /// </summary>
    /// <param name="appSettings">The AppSettings object to tokenize.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>String representation of tokenized AppSettings.</returns>
    string BuildTokenizedAppSettingsAsync(AppSettings appSettings, CancellationToken cancellationToken = default);
}
