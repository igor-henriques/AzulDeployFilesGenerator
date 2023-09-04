namespace AzulDeployFilesGenerator.Application.Factories;

internal sealed class ExcelSheetFactory : IExcelSheetFactory
{
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly IOptions<ApplicationDefaultsOptions> _appDefaultsOptions;
    private readonly ILogger<ExcelSheetFactory> _logger;
    private readonly ISolutionFilesService _solutionFilesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelSheetFactory"/> class.
    /// </summary>
    /// <param name="cliOptions">The CLI options.</param>
    /// <param name="solutionFilesService">The service for handling solution files.</param>
    /// <param name="appDefaultsOptions">The default application options.</param>
    public ExcelSheetFactory(
        IOptions<CliCommandOptions> cliOptions,
        ISolutionFilesService solutionFilesService,
        IOptions<ApplicationDefaultsOptions> appDefaultsOptions,
        ILogger<ExcelSheetFactory> logger)
    {
        _cliOptions = cliOptions ?? throw new ArgumentNullException(nameof(cliOptions));
        _solutionFilesService = solutionFilesService ?? throw new ArgumentNullException(nameof(solutionFilesService));
        _appDefaultsOptions = appDefaultsOptions ?? throw new ArgumentNullException(nameof(appDefaultsOptions));

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _logger = logger;
    }

    /// <summary>
    /// Builds an Excel sheet based on the application settings.
    /// </summary>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Excel package containing all the information.</returns>
    public async Task<ExcelPackage> BuildExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        ExcelPackage excelPackage = new();

        GenerateRepositoryInfoWorksheet(excelPackage, cancellationToken);
        GenerateDockerImageWorksheet(excelPackage);
        GenerateTokenizerWorksheet(excelPackage, appSettings);

        if (_cliOptions.Value.ApplicationType is EApplicationType.Api)
        {
            GenerateApiGatewayWorksheet(excelPackage);
        }

        if (_cliOptions.Value.ApplicationType is EApplicationType.Consumer or EApplicationType.CronJob)
        {
            await GenerateInfraAsCodeWorksheet(excelPackage, appSettings, cancellationToken);
        }

        return excelPackage;
    }

    /// <summary>
    /// Generates the 'Infra As Code' worksheet in the Excel package.
    /// </summary>
    /// <param name="excelPackage">The Excel package object.</param>
    /// <param name="appSettings">The application settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task GenerateInfraAsCodeWorksheet(ExcelPackage excelPackage, AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        const string worksheetName = "Infra As Code";
        
        var hasSubscribers = await _solutionFilesService.HasAnySubscribers(cancellationToken: cancellationToken);
        var hasPublishers = await _solutionFilesService.HasAnyPublishers(cancellationToken: cancellationToken);

        var variables = appSettings.GetRawEnvVariables(considerExcelExceptionFields: true);        

        if (!hasSubscribers && !hasPublishers)
        {
            _logger.LogWarning("No subscribers or publishers were found in the solution. Skipping {worksheetName} worksheet.\n", 
                worksheetName);

            return;            
        }

        var infraAsCodeWorksheet = excelPackage.Workbook.Worksheets.Add(worksheetName);

        if (hasPublishers)
        {
            infraAsCodeWorksheet.Cells["A1"].Value = "Publisher";

            infraAsCodeWorksheet.Cells["A2"].Value = "Variável (Aba Tokenizer)";
            infraAsCodeWorksheet.Cells["B2"].Value = "Aplicação";
            infraAsCodeWorksheet.Cells["C2"].Value = "Topic Name";
            infraAsCodeWorksheet.Cells["D2"].Value = "Descrição";

            infraAsCodeWorksheet.Cells["A3"].Value = "serviceBusSettings.publisherTopic";
            infraAsCodeWorksheet.Cells["B3"].Value = _cliOptions.Value.ApplicationName;
            infraAsCodeWorksheet.Cells["C3"].Value = appSettings.ServiceBusSettings.PublisherTopic;
            infraAsCodeWorksheet.Cells["D3"].Value = ""; //Empty description for now

            infraAsCodeWorksheet.SetBackgroundColor("A3:D3", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone);
        }

        if (hasSubscribers)
        {
            int startRowPosition = hasPublishers ? 5 : 1;

            infraAsCodeWorksheet.Cells[$"A{startRowPosition}"].Value = "Subscriber";

            infraAsCodeWorksheet.Cells[$"A{startRowPosition + 1}"].Value = "Variável (Aba Tokenizer)";
            infraAsCodeWorksheet.Cells[$"B{startRowPosition + 1}"].Value = "Subscription Name";
            infraAsCodeWorksheet.Cells[$"C{startRowPosition + 1}"].Value = "Topic Name";
            infraAsCodeWorksheet.Cells[$"D{startRowPosition + 1}"].Value = "Dead Lettering Expiration";
            infraAsCodeWorksheet.Cells[$"E{startRowPosition + 1}"].Value = "Max Delivery";
            infraAsCodeWorksheet.Cells[$"F{startRowPosition + 1}"].Value = "Message Sessions";
            infraAsCodeWorksheet.Cells[$"G{startRowPosition + 1}"].Value = "Sql Filter";
            infraAsCodeWorksheet.Cells[$"H{startRowPosition + 1}"].Value = "Auto Delete";

            foreach (var eventModel in appSettings.Events.Select((Event, Index) => (Event, Index)))
            {
                var index = eventModel.Index + 2;

                var connectionStringVariable = variables?.Where(var => var.Name.Contains(eventModel.Event.Id)
                    && var.Value.Contains(eventModel.Event.ConnectionString))?.FirstOrDefault()?.Name;

                var subscription = variables.FirstOrDefault(var => var.Name == $"events.{eventModel.Event.Id}.parameters.Subscription").Name;
                var topic = variables.FirstOrDefault(var => var.Name == $"events.{eventModel.Event.Id}.parameters.Topic").Name;
                var maxConcurrentCalls = "eventCustomSettings.AzureServiceBusSettings.MaxConcurrentCalls";

                infraAsCodeWorksheet.Cells[$"A{startRowPosition + index}"].Value = connectionStringVariable;
                infraAsCodeWorksheet.Cells[$"B{startRowPosition + index}"].Value = subscription;
                infraAsCodeWorksheet.Cells[$"C{startRowPosition + index}"].Value = topic;
                infraAsCodeWorksheet.Cells[$"D{startRowPosition + index}"].Value = "false";
                infraAsCodeWorksheet.Cells[$"E{startRowPosition + index}"].Value = maxConcurrentCalls;
                infraAsCodeWorksheet.Cells[$"F{startRowPosition + index}"].Value = "false";
                infraAsCodeWorksheet.Cells[$"G{startRowPosition + index}"].Value = "1=1";
                infraAsCodeWorksheet.Cells[$"H{startRowPosition + index}"].Value = "false";

                infraAsCodeWorksheet.SetBackgroundColor($"A{startRowPosition + index}:H{startRowPosition + index}", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone);
            }
        }

        infraAsCodeWorksheet.SetGlobalStyling(_appDefaultsOptions.Value.ExcelFontName);
        infraAsCodeWorksheet.SetBorder("A1");

        if (hasSubscribers && hasPublishers)
        {
            infraAsCodeWorksheet.SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "A2:D2", "A5", "A6:H6");
            infraAsCodeWorksheet.SetBorder("A2:D2");
            infraAsCodeWorksheet.SetBorder("A5");
            infraAsCodeWorksheet.SetBorder("A6:H6");
        }
        else if (hasSubscribers)
        {
            infraAsCodeWorksheet.SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "A2:H2");
            infraAsCodeWorksheet.SetBorder("A2:H2");
            infraAsCodeWorksheet.SetBorder("A3:H3");
        }
        else if (hasPublishers)
        {
            infraAsCodeWorksheet.SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "A2:D2");
            infraAsCodeWorksheet.SetBorder("A2:D2");
            infraAsCodeWorksheet.SetBorder("A3:D3");
        }

        (var firstCell, var lastCell) = infraAsCodeWorksheet.GetWorksheetDataRange();

        infraAsCodeWorksheet.AutoFitColumns($"{firstCell}:{lastCell}", maxWidth: _appDefaultsOptions.Value.ExcelColumnWidth);
    }

    /// <summary>
    /// Generates the 'Api Gateway' worksheet in the Excel package.
    /// </summary>
    /// <param name="excelPackage">The Excel package object.</param>
    private void GenerateApiGatewayWorksheet(ExcelPackage excelPackage)
    {
        var apiGatewayWorksheet = excelPackage.Workbook.Worksheets.Add("Api Gateway");

        apiGatewayWorksheet.Cells["A1"].Value = "Products";
        apiGatewayWorksheet.Cells["A2"].Value = _cliOptions.Value.ApplicationName;

        apiGatewayWorksheet.Cells["B1"].Value = "Subscription Required";
        apiGatewayWorksheet.Cells["B2"].Value = "true";

        apiGatewayWorksheet.SetBorder("A1:B2");

        apiGatewayWorksheet.Cells["A5"].Value = "Environment";
        apiGatewayWorksheet.Cells["A6"].Value = "TST";
        apiGatewayWorksheet.Cells["A7"].Value = "STG";
        apiGatewayWorksheet.Cells["A8"].Value = "PRD";

        apiGatewayWorksheet.Cells["B5"].Value = "Ips Policy";

        apiGatewayWorksheet.Cells["C5"].Value = "Public e/ou Private";
        apiGatewayWorksheet.Cells["C6"].Value = "public";
        apiGatewayWorksheet.Cells["C7"].Value = "public";
        apiGatewayWorksheet.Cells["C8"].Value = "public";

        apiGatewayWorksheet.Cells["D5"].Value = "Variavel";
        apiGatewayWorksheet.Cells["D6"].Value = "swaggerdoc.host";
        apiGatewayWorksheet.Cells["D7"].Value = "swaggerdoc.host";
        apiGatewayWorksheet.Cells["D8"].Value = "swaggerdoc.host";

        apiGatewayWorksheet.Cells["E5"].Value = "Service";
        apiGatewayWorksheet.Cells["E6"].Value = $"{_cliOptions.Value.DeployName}-tst";
        apiGatewayWorksheet.Cells["E7"].Value = $"{_cliOptions.Value.DeployName}-stg";
        apiGatewayWorksheet.Cells["E8"].Value = $"{_cliOptions.Value.DeployName}-prd";

        apiGatewayWorksheet.SetGlobalStyling(_appDefaultsOptions.Value.ExcelFontName);
        apiGatewayWorksheet.SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "B1", "A5", "B5", "C5", "D5", "E5");
        apiGatewayWorksheet.SetBorder("A5:E8");

        apiGatewayWorksheet.SetBackgroundColor("A2", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("B2", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("A6:A8", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("B6:B8", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("C6:C8", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("D6:D8", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetBackgroundColor("E6:E8", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .AutoFitColumns("A1:E8", maxWidth: _appDefaultsOptions.Value.ExcelColumnWidth);
    }

    /// <summary>
    /// Generates the 'Repository Info' worksheet in the Excel package.
    /// </summary>
    /// <param name="excelPackage">The Excel package object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private void GenerateRepositoryInfoWorksheet(ExcelPackage excelPackage, CancellationToken cancellationToken = default)
    {
        (var parentDirectory, var csprojName) = _solutionFilesService.FindEntrypointAssemblyAsync(cancellationToken: cancellationToken);

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

        repositoryWorksheet.SetBackgroundColor("A17", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone);
        repositoryWorksheet.Cells["B17"].Value = "Novas variáveis e valores";

        repositoryWorksheet.SetBackgroundColor("A18", _appDefaultsOptions.Value.ExcelDefaultYellowColorTone);
        repositoryWorksheet.Cells["B18"].Value = "Alterar valores";

        repositoryWorksheet.SetBackgroundColor("A19", _appDefaultsOptions.Value.ExcelDefaultOrangeColorTone);
        repositoryWorksheet.Cells["B19"].Value = "Criar/Alterar valores que vão ser definidos pela arquitetura";

        repositoryWorksheet.SetBackgroundColor("A20", _appDefaultsOptions.Value.ExcelDefaultRedColorTone);
        repositoryWorksheet.Cells["B20"].Value = "Remover variaveis e valores";

        repositoryWorksheet.SetGlobalStyling(_appDefaultsOptions.Value.ExcelFontName)
            .SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "B1", "A16", "B16")
            .SetBorder("A1:B20")
            .AutoFitColumns("A1:B20", maxWidth: _appDefaultsOptions.Value.ExcelColumnWidth);
    }

    /// <summary>
    /// Generates the 'Docker Image' worksheet in the Excel package.
    /// </summary>
    /// <param name="excelPackage">The Excel package object.</param>
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

        dockerWorksheet.SetGlobalStyling(_appDefaultsOptions.Value.ExcelFontName)
            .SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "B1", "C1", "D1")
            .SetBorder("A1:D2")
            .AutoFitColumns("A1:D2", maxWidth: _appDefaultsOptions.Value.ExcelColumnWidth)
            .SetBackgroundColor("A2:D2", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone);
    }

    /// <summary>
    /// Generates the 'Tokenizer' worksheet in the Excel package.
    /// </summary>
    /// <param name="excelPackage">The Excel package object.</param>
    /// <param name="appSettings">The application settings.</param>
    private void GenerateTokenizerWorksheet(ExcelPackage excelPackage, AppSettings appSettings)
    {
        var tokenizerWorksheet = excelPackage.Workbook.Worksheets.Add("Tokenizer");

        tokenizerWorksheet.Cells["A1"].Value = "Arquivo Tokenizado";
        tokenizerWorksheet.Cells["B1"].Value = "Variables";
        tokenizerWorksheet.Cells["C1"].Value = "TST";
        tokenizerWorksheet.Cells["D1"].Value = "STG";
        tokenizerWorksheet.Cells["E1"].Value = "PRD";

        var variables = appSettings.GetRawEnvVariables(considerExcelExceptionFields: true);
        var tokenizedFilePath = $"{_cliOptions.Value.ApplicationName}/src/{Constants.FileNames.K8sYaml}";

        tokenizerWorksheet.Cells["A2"].Value = tokenizedFilePath;
        tokenizerWorksheet.Cells["B2"].Value = "environment";
        tokenizerWorksheet.Cells["C2"].Value = "TST";
        tokenizerWorksheet.Cells["D2"].Value = "STG";
        tokenizerWorksheet.Cells["E2"].Value = "PRD";

        foreach (var variableModel in variables.Select((Variable, Index) => (Variable, Index)))
        {
            var variable = variableModel.Variable;
            var index = variableModel.Index + 3;

            tokenizerWorksheet.Cells[$"A{index}"].Value = tokenizedFilePath;
            tokenizerWorksheet.Cells[$"B{index}"].Value = variable.Name;
            tokenizerWorksheet.Cells[$"C{index}"].Value = variable.Value.Replace("'", string.Empty);
            tokenizerWorksheet.Cells[$"D{index}"].Value = variable.Value.Replace("'", string.Empty);
            tokenizerWorksheet.Cells[$"E{index}"].Value = variable.Value.Replace("'", string.Empty);
        }

        (var firstCell, var lastCell) = tokenizerWorksheet.GetWorksheetDataRange();

        tokenizerWorksheet.SetBackgroundColor($"{firstCell}:{lastCell}", _appDefaultsOptions.Value.ExcelDefaultGreenColorTone)
            .SetGlobalStyling(_appDefaultsOptions.Value.ExcelFontName)
            .SetMenuDefaultStyling(_appDefaultsOptions.Value.ExcelMenuBackgroundColor, "A1", "B1", "C1", "D1", "E1")
            .SetBorder($"{firstCell}:{lastCell}")
            .AutoFitColumns($"{firstCell}:{lastCell}", maxWidth: _appDefaultsOptions.Value.ExcelColumnWidth);
    }
}

internal static class ExcelSheetFactoryExtensions
{
    /// <summary>
    /// Sets the background color of the specified cell.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cell">The cell reference as a string.</param>
    /// <param name="color">The color to set.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetBackgroundColor(this ExcelWorksheet worksheet, string cell, Color color)
    {
        worksheet.Cells[cell].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[cell].Style.Fill.BackgroundColor.SetColor(color);
        return worksheet;
    }

    /// <summary>
    /// Sets the font color of the specified cell.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cell">The cell reference as a string.</param>
    /// <param name="color">The color to set.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetFontColor(this ExcelWorksheet worksheet, string cell, Color color)
    {
        worksheet.Cells[cell].Style.Font.Color.SetColor(color);
        return worksheet;
    }

    /// <summary>
    /// Sets the font size of the specified cell.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cell">The cell reference as a string.</param>
    /// <param name="fontSize">The font size to set.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetFontSize(this ExcelWorksheet worksheet, string cell, float fontSize)
    {
        worksheet.Cells[cell].Style.Font.Size = fontSize;
        return worksheet;
    }

    /// <summary>
    /// Makes the font of the specified cell bold.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cell">The cell reference as a string.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetBoldFont(this ExcelWorksheet worksheet, string cell)
    {
        worksheet.Cells[cell].Style.Font.Bold = true;
        return worksheet;
    }

    /// <summary>
    /// Sets the font family of the specified cell.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cell">The cell reference as a string.</param>
    /// <param name="fontName">The name of the font.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetFontFamily(this ExcelWorksheet worksheet, string cell, string fontName)
    {
        worksheet.Cells[cell].Style.Font.Name = fontName;
        return worksheet;
    }

    /// <summary>
    /// Sets default styling for menu cells.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="backgroundColor">The background color for the menu.</param>
    /// <param name="cells">An array of cell references to style.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetMenuDefaultStyling(this ExcelWorksheet worksheet, Color backgroundColor, params string[] cells)
    {
        foreach (var cell in cells)
        {
            worksheet.SetFontFamily(cell, Constants.ExcelDefaults.DEFAULT_FONT_NAME);
            worksheet.SetBoldFont(cell);
            worksheet.SetFontColor(cell, Color.White);
            worksheet.SetBackgroundColor(cell, backgroundColor);
            worksheet.SetFontSize(cell, 12);
        }

        return worksheet;
    }

    /// <summary>
    /// Sets global styling for the worksheet.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="fontName">The name of the font to use.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetGlobalStyling(this ExcelWorksheet worksheet, string fontName)
    {
        var startCell = worksheet.Dimension.Start;
        var endCell = worksheet.Dimension.End;

        for (int row = startCell.Row; row <= endCell.Row; row++)
        {
            for (int col = startCell.Column; col <= endCell.Column; col++)
            {
                worksheet.Cells[row, col].Style.Font.Name = fontName;
                worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }

        return worksheet;
    }

    /// <summary>
    /// Adds borders to the specified cells.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cells">The range of cells to add borders to.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet SetBorder(this ExcelWorksheet worksheet, string cells)
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

        return worksheet;
    }

    /// <summary>
    /// Auto-fits the columns for the specified cell range.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <param name="cells">The range of cells to auto-fit.</param>
    /// <param name="maxWidth">The maximum width for any column.</param>
    /// <returns>The updated ExcelWorksheet object.</returns>
    public static ExcelWorksheet AutoFitColumns(this ExcelWorksheet worksheet, string cells, double? maxWidth = Constants.ExcelDefaults.MAX_COLUMN_WIDTH)
    {
        string[] parts = cells.Split(':');
        string startCell = parts[0];
        string endCell = parts[1];

        ExcelCellAddress startAddress = new(startCell);
        ExcelCellAddress endAddress = new(endCell);

        Dictionary<int, double> colMaxWidth = new();

        for (int row = startAddress.Row; row <= endAddress.Row; row++)
        {
            for (int col = startAddress.Column; col <= endAddress.Column; col++)
            {
                object cellValue = worksheet.Cells[row, col].Value;
                double requiredWidth = cellValue != null ? cellValue.ToString().Length : 0;

                if (!colMaxWidth.ContainsKey(col) || requiredWidth > colMaxWidth[col])
                {
                    if (maxWidth.HasValue)
                    {
                        colMaxWidth[col] = requiredWidth > maxWidth.Value
                           ? maxWidth.Value
                           : requiredWidth;
                    }
                    else
                    {
                        colMaxWidth[col] = requiredWidth;
                    }
                }
            }
        }

        foreach (var col in colMaxWidth.Keys)
        {
            worksheet.Column(col).Width = colMaxWidth[col];
        }

        return worksheet;
    }

    /// <summary>
    /// Gets the data range for the worksheet.
    /// </summary>
    /// <param name="worksheet">The worksheet object.</param>
    /// <returns>A tuple containing the start and end cell references.</returns>
    public static (string, string) GetWorksheetDataRange(this ExcelWorksheet worksheet)
    {
        int startRow = worksheet.Dimension.Start.Row;
        int startColumn = worksheet.Dimension.Start.Column;
        int endRow = worksheet.Dimension.End.Row;
        int endColumn = worksheet.Dimension.End.Column;

        string startCell = worksheet.Cells[startRow, startColumn].Address;
        string endCell = worksheet.Cells[endRow, endColumn].Address;

        return (startCell, endCell);
    }
}