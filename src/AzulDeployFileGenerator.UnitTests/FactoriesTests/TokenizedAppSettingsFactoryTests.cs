namespace AzulDeployFileGenerator.UnitTests.FactoriesTests;

public sealed class TokenizedAppSettingsFactoryTests
{
    private readonly TokenizedAppSettingsFactory _factory;

    public TokenizedAppSettingsFactoryTests()
    {
        _factory = new TokenizedAppSettingsFactory();
    }

    [Fact]
    public void BuildTokenizedAppSettingsAsync_Should_Handle_Nested_Properties()
    {
        // Arrange
        var log = new Log()
        {
            Console = new Domain.Models.AppSettingsObjects.Console()
            {
                Enabled = true,
                MinimumLevel = "Information"
            }
        };
        var apiSettings = new ApiSettings()
        {
            ShowDetailedException = true
        };
        var appSettings = new AppSettings { Log = log, ApiSettings = apiSettings };

        // Act
        string result = _factory.BuildTokenizedAppSettingsAsync(appSettings);

        // Assert
        JObject jsonResult = JObject.Parse(result);
        jsonResult["log"]?["console"]?["minimumLevel"]?.ToObject<string>().Should().Be("${log.console.minimumLevel}");
        jsonResult["apiSettings"]?["showDetailedException"]?.ToObject<string>().Should().Be("${apiSettings.showDetailedException}");
    }

    [Fact]
    public void BuildTokenizedAppSettingsAsync_Should_Handle_Lists()
    {
        // Arrange
        var connectionSettings = new List<ConnectionSetting>
        {
            new ConnectionSetting { Id = "conn1", ConnectionString = "connString1" },
            new ConnectionSetting { Id = "conn2", ConnectionString = "connString2" }
        };

        var appSettings = new AppSettings { ConnectionSettings = connectionSettings };

        // Act
        string result = _factory.BuildTokenizedAppSettingsAsync(appSettings);

        // Assert
        JObject jsonResult = JObject.Parse(result);
        jsonResult["connectionSettings"]?[0]?["id"]?.ToObject<string>().Should().Be("conn1");
        jsonResult["connectionSettings"]?[0]?["connectionString"]?.ToObject<string>().Should().Be("${connectionSettings.conn1.connectionString}");
        jsonResult["connectionSettings"]?[1]?["id"]?.ToObject<string>().Should().Be("conn2");
        jsonResult["connectionSettings"]?[1]?["connectionString"]?.ToObject<string>().Should().Be("${connectionSettings.conn2.connectionString}");
    }
}
