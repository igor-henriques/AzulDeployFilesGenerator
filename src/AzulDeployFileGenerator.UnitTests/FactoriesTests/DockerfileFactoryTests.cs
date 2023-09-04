namespace AzulDeployFileGenerator.UnitTests.FactoriesTests;

public sealed class DockerfileFactoryTests
{
    private readonly Mock<IOptions<CliCommandOptions>> _mockCliOptions;
    private readonly Mock<ISolutionFilesService> _mockSolutionFilesService;
    private readonly Mock<IOptions<ApplicationDefaultsOptions>> _mockAppDefaultsOptions;    

    public DockerfileFactoryTests()
    {
        _mockCliOptions = new Mock<IOptions<CliCommandOptions>>();
        _mockSolutionFilesService = new Mock<ISolutionFilesService>();
        _mockAppDefaultsOptions = new Mock<IOptions<ApplicationDefaultsOptions>>();
    }

    [Fact]
    public async Task BuildAzulDockerfile_Should_Return_Correct_Dockerfile_When_Api_Type()
    {
        // Arrange
        SetupMocksForApiType();
        var factory = new DockerfileFactory(_mockSolutionFilesService.Object, _mockCliOptions.Object, _mockAppDefaultsOptions.Object);

        // Act
        var result = await factory.BuildAzulDockerfile();

        // Assert
        result.Should().Contain($"EXPOSE {Constants.DEFAULT_EXPOSED_API_PORT_DOCKERFILE}"); 
    }

    [Fact]
    public async Task BuildAzulDockerfile_Should_Not_Contain_Build_Command_When_Api_Type()
    {
        // Arrange
        SetupMocksForApiType();
        var factory = new DockerfileFactory(_mockSolutionFilesService.Object, _mockCliOptions.Object, _mockAppDefaultsOptions.Object);

        // Act
        var result = await factory.BuildAzulDockerfile();

        // Assert
        result.Should().NotContain("RUN dotnet build");
    }

    [Fact]
    public async Task BuildAzulDockerfile_Should_Contain_Build_Command_When_Consumer_Type()
    {
        // Arrange
        SetupMocksForConsumerType();
        var factory = new DockerfileFactory(_mockSolutionFilesService.Object, _mockCliOptions.Object, _mockAppDefaultsOptions.Object);

        // Act
        var result = await factory.BuildAzulDockerfile();

        // Assert
        result.Should().Contain("RUN dotnet build");
    }

    private void SetupMocksForApiType()
    {
        var cliOptions = new CliCommandOptions().SetApplicationType("api");
        _mockCliOptions.Setup(m => m.Value).Returns(cliOptions);

        var appDefaults = new ApplicationDefaultsOptions { ExposedApiPortDockerfile = Constants.DEFAULT_EXPOSED_API_PORT_DOCKERFILE };
        _mockAppDefaultsOptions.Setup(m => m.Value).Returns(appDefaults);
    }

    private void SetupMocksForConsumerType()
    {
        // Mock CliCommandOptions for Consumer scenario
        var cliOptions = new CliCommandOptions().SetApplicationType("consumer");
        _mockCliOptions.Setup(m => m.Value).Returns(cliOptions);

        var appDefaults = new ApplicationDefaultsOptions();
        _mockAppDefaultsOptions.Setup(m => m.Value).Returns(appDefaults);
    }
}
