namespace AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;

public sealed record CustomMiddleware
{
    [JsonProperty("exceptionHandler")]
    public bool ExceptionHandler { get; set; }

    [JsonProperty("requestTracking")]
    public bool RequestTracking { get; set; }

    [JsonProperty("tokenValidation")]
    public bool TokenValidation { get; set; }

    [JsonProperty("cultureHandler")]
    public bool CultureHandler { get; set; }
}
