namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IExcelSheetFactory
{
    Task<ExcelPackage> BuildExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default);
}
