namespace AzulDeployFilesGenerator.Application.Factories;

internal sealed class ExcelSheetFactory : IExcelSheetFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;

    public ExcelSheetFactory(
        IOptions<CliCommandOptions> cliOptions,
        ISolutionFilesService solutionFilesService)
    {
        _cliOptions = cliOptions;
        _solutionFilesService = solutionFilesService;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ExcelPackage> BuildExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        ExcelPackage excelPackage = new();

        await GenerateRepositoryInfoWorksheet(excelPackage, cancellationToken);
        GenerateDockerImageWorksheet(excelPackage);
        GenerateInfraAsCodeWorksheet(excelPackage, appSettings);

        return excelPackage;
    }

    private async Task GenerateRepositoryInfoWorksheet(ExcelPackage excelPackage, CancellationToken cancellationToken = default)
    {
        (var parentDirectory, var csprojName) = await _solutionFilesService.FindEntrypointAssemblyAsync(cancellationToken: cancellationToken);

        var repositoryWorksheet = excelPackage.Workbook.Worksheets.Add("Dados Repositorio");

        repositoryWorksheet.Cells["A1"].Value = "Nome";
        repositoryWorksheet.Cells["B1"].Value = "Descrição";

        repositoryWorksheet.Cells["A2"].Value = "Projeto";
        repositoryWorksheet.Cells["B2"].Value = _cliOptions.Value.ApplicationName;

        repositoryWorksheet.Cells["A3"].Value = "Nome_Fabrica";
        repositoryWorksheet.Cells["B3"].Value = "Online Applications";

        repositoryWorksheet.Cells["A4"].Value = "Telefone_Fabrica";
        repositoryWorksheet.Cells["B4"].Value = "55 19 55194062-8611";

        repositoryWorksheet.Cells["A5"].Value = "Email_Fabrica";
        repositoryWorksheet.Cells["B5"].Value = "barramentomnt@onlineapp.com.br";

        repositoryWorksheet.Cells["A6"].Value = "Repo_Path";
        repositoryWorksheet.Cells["B6"].Value = _cliOptions.Value.ApplicationName;

        repositoryWorksheet.Cells["A7"].Value = "Repo_Path_Dockerfile";
        repositoryWorksheet.Cells["B7"].Value = "src/Dockerfile";

        repositoryWorksheet.Cells["A8"].Value = "Repo_Path_K8Syml";
        repositoryWorksheet.Cells["B8"].Value = "src/k8sdeploy.yaml";

        repositoryWorksheet.Cells["A9"].Value = "Repo_Path_Swagger";

        repositoryWorksheet.Cells["A10"].Value = "Repo_Path_CSProj";
        repositoryWorksheet.Cells["B10"].Value = $"{_cliOptions.Value.ApplicationName}/src/{parentDirectory}/{csprojName}";

        repositoryWorksheet.Cells["A11"].Value = "Nome Deploy API K8S";

        repositoryWorksheet.Cells["A12"].Value = "Apigateway";

        repositoryWorksheet.Cells["A13"].Value = "URL";
        repositoryWorksheet.Cells["A14"].Value = "URL Suffix";

        if (_cliOptions.Value.ApplicationType is EApplicationType.Api)
        {
            repositoryWorksheet.Cells["B9"].Value = "src/Api/Swagger/Definition";
            repositoryWorksheet.Cells["B11"].Value = $"{_cliOptions.Value.DeployName}-__environment__";
            repositoryWorksheet.Cells["B12"].Value = "ApiM-[env]-Us-General";
            repositoryWorksheet.Cells["B13"].Value = "[PREENCHIDO POR ARQUITETURA]";
            repositoryWorksheet.Cells["B14"].Value = "[PREENCHIDO POR DEV]";
        }

        repositoryWorksheet.Cells["A16"].Value = "Cor";
        repositoryWorksheet.Cells["B16"].Value = "Ações na planilha";

        repositoryWorksheet.SetBackgroundColor("A17", Color.LimeGreen);
        repositoryWorksheet.Cells["B17"].Value = "Novas variáveis e valores";

        repositoryWorksheet.SetBackgroundColor("A18", Color.Yellow);
        repositoryWorksheet.Cells["B18"].Value = "Alterar valores";

        repositoryWorksheet.SetBackgroundColor("A19", Color.Orange);
        repositoryWorksheet.Cells["B19"].Value = "Criar/Alterar valores que vão ser definidos pela arquitetura";

        repositoryWorksheet.SetBackgroundColor("A20", Color.Red);
        repositoryWorksheet.Cells["B20"].Value = "Remover variaveis e valores";

        repositoryWorksheet.SetGlobalStyling();
        repositoryWorksheet.SetMenuDefaultStyling("A1", "B1", "A16", "B16");
        repositoryWorksheet.SetBorder("A1:B20");
        repositoryWorksheet.AutoFitColumns("A1:B20");
    }

    private void GenerateDockerImageWorksheet(ExcelPackage excelPackage)
    {
        var dockerWorksheet = excelPackage.Workbook.Worksheets.Add("Imagem");

        dockerWorksheet.Cells["A1"].Value = "Aplicação";
        dockerWorksheet.Cells["A2"].Value = _cliOptions.Value.ApplicationName;

        dockerWorksheet.Cells["B1"].Value = "Arquivo";
        dockerWorksheet.Cells["B2"].Value = $"{_cliOptions.Value.ApplicationName}/src/Dockerfile";

        dockerWorksheet.Cells["C1"].Value = "Imagem";
        dockerWorksheet.Cells["C2"].Value = _cliOptions.Value.ImageName;

        dockerWorksheet.Cells["D1"].Value = "Descrição";
        dockerWorksheet.Cells["D2"].Value = Constants.Messages.DEFAULT_IMAGE_TOKENIZER_DESCRIPTION;

        dockerWorksheet.SetGlobalStyling();
        dockerWorksheet.SetMenuDefaultStyling("A1", "B1", "C1", "D1");
        dockerWorksheet.SetBorder("A1:D2");
        dockerWorksheet.AutoFitColumns("A1:D2");
    }

    private void GenerateInfraAsCodeWorksheet(ExcelPackage excelPackage, AppSettings appSettings)
    {

    }
}

internal static class ExcelSheetFactoryExtensions
{
    public static void SetBackgroundColor(this ExcelWorksheet worksheet, string cell, Color color)
    {
        worksheet.Cells[cell].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[cell].Style.Fill.BackgroundColor.SetColor(color);
    }

    public static void SetFontColor(this ExcelWorksheet worksheet, string cell, Color color)
    {
        worksheet.Cells[cell].Style.Font.Color.SetColor(color);
    }

    public static void SetFontSize(this ExcelWorksheet worksheet, string cell, float fontSize)
    {
        worksheet.Cells[cell].Style.Font.Size = fontSize;
    }

    public static void SetBoldFont(this ExcelWorksheet worksheet, string cell)
    {
        worksheet.Cells[cell].Style.Font.Bold = true;
    }

    public static void SetFontFamily(this ExcelWorksheet worksheet, string cell, string fontName)
    {
        worksheet.Cells[cell].Style.Font.Name = fontName;
    }

    public static void SetMenuDefaultStyling(this ExcelWorksheet worksheet, params string[] cells)
    {
        foreach (var cell in cells)
        {
            worksheet.SetFontFamily(cell, Constants.ExcelDefaults.DEFAULT_FONT_NAME);
            worksheet.SetBoldFont(cell);
            worksheet.SetFontColor(cell, Color.White);
            worksheet.SetBackgroundColor(cell, Constants.ExcelDefaults.DEFAULT_MENU_BACKGROUND_COLOR);
            worksheet.SetFontSize(cell, 12);
        }
    }

    public static void SetGlobalStyling(this ExcelWorksheet worksheet)
    {
        var startCell = worksheet.Dimension.Start;
        var endCell = worksheet.Dimension.End;

        for (int row = startCell.Row; row <= endCell.Row; row++)
        {
            for (int col = startCell.Column; col <= endCell.Column; col++)
            {
                worksheet.Cells[row, col].Style.Font.Name = Constants.ExcelDefaults.DEFAULT_FONT_NAME;
                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }
    }

    public static void SetBorder(this ExcelWorksheet worksheet, string cells)
    {
        var cell = worksheet.Cells[cells];

        var border = cell.Style.Border;
        border.Top.Style = ExcelBorderStyle.Thin;
        border.Bottom.Style = ExcelBorderStyle.Thin;
        border.Left.Style = ExcelBorderStyle.Thin;
        border.Right.Style = ExcelBorderStyle.Thin;

        border.Top.Color.SetColor(Color.Black);
        border.Bottom.Color.SetColor(Color.Black);
        border.Left.Color.SetColor(Color.Black);
        border.Right.Color.SetColor(Color.Black);
    }

    public static void AutoFitColumns(this ExcelWorksheet worksheet, string cells, double maxWidth = 0)
    {        
        string[] parts = cells.Split(':');
        string startCell = parts[0];
        string endCell = parts[1];
        
        ExcelCellAddress startAddress = new(startCell);
        ExcelCellAddress endAddress = new(endCell);

        Dictionary<int, double> colMaxWidth = new ();

        for (int row = startAddress.Row; row <= endAddress.Row; row++)
        {
            for (int col = startAddress.Column; col <= endAddress.Column; col++)
            {
                object cellValue = worksheet.Cells[row, col].Value;
                double requiredWidth = cellValue != null ? cellValue.ToString().Length : 0;

                if (!colMaxWidth.ContainsKey(col) || requiredWidth > colMaxWidth[col])
                {
                    colMaxWidth[col] = requiredWidth > maxWidth
                        ? maxWidth
                        : requiredWidth;
                }
            }
        }

        foreach (var col in colMaxWidth.Keys)
        {
            worksheet.Column(col).Width = colMaxWidth[col];
        }
    }
}