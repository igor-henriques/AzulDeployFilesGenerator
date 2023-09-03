namespace AzulDeployFileGenerator.Domain.Models.Options;

public sealed record ApplicationDefaultsOptions
{
    [JsonConverter(typeof(ColorJsonConverter))]
    public Color ExcelMenuBackgroundColor { get; init; } = Constants.ExcelDefaults.DEFAULT_MENU_BACKGROUND_COLOR;

    [JsonConverter(typeof(ColorJsonConverter))]
    public Color ExcelDefaultGreenColorTone { get; init; } = Constants.ExcelDefaults.DEFAULT_GREEN_COLOR_TONE;

    [JsonConverter(typeof(ColorJsonConverter))]
    public Color ExcelDefaultOrangeColorTone { get; init; } = Constants.ExcelDefaults.DEFAULT_ORANGE_COLOR_TONE;

    [JsonConverter(typeof(ColorJsonConverter))]
    public Color ExcelDefaultYellowColorTone { get; init; } = Constants.ExcelDefaults.DEFAULT_YELLOW_COLOR_TONE;

    [JsonConverter(typeof(ColorJsonConverter))]
    public Color ExcelDefaultRedColorTone { get; init; } = Constants.ExcelDefaults.DEFAULT_RED_COLOR_TONE;

    public string ExcelFontName { get; init; } = Constants.ExcelDefaults.DEFAULT_FONT_NAME;
    public int ExcelColumnWidth { get; init; } = Constants.ExcelDefaults.MAX_COLUMN_WIDTH;
    public string OnlineKubernetesNamespace { get; init; } = Constants.DEFAULT_ONLINE_KUBERNETES_NAMESPACE;
    public string AzulAcrName { get; init; } = Constants.ImageNames.BASE_AZUL_ACR_NAME;
    public string OnlineAcrName { get; init; } = Constants.ImageNames.BASE_ONLINE_ACR_NAME;
    public string AzulDotNetSdkImage { get; init; } = Constants.ImageNames.AZUL_DOTNET_CORE_SDK;
    public string AzulAspNetSdkImage { get; init; } = Constants.ImageNames.AZUL_ASPNET_CORE;
    public string OnlineDotNetSdkImage { get; init; } = Constants.ImageNames.ISABKO_DOTNET_CORE_SDK;
    public string OnlineAspNetSdkImage { get; init; } = Constants.ImageNames.ISABKO_ASPNET_CORE;
    public string AzulFrameworkNugetKey { get; init; }
}