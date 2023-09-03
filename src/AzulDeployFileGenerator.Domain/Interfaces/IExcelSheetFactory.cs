namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IExcelSheetFactory
{
    /// <summary>
    /// Builds an Excel sheet based on the application settings.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Excel package containing all the information.</returns>
    Task<ExcelPackage> BuildExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default);
}
