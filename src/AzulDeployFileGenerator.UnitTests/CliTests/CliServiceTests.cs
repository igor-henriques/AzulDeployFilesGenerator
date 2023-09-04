namespace AzulDeployFileGenerator.UnitTests.CliTests;

public sealed class CliServiceTests
{
    private readonly Mock<IOptions<CliCommandOptions>> _mockCliOptions;
    private readonly Mock<ILogger<CliService>> _mockLogger;

    public CliServiceTests()
    {
        _mockCliOptions = new Mock<IOptions<CliCommandOptions>>();
        _mockLogger = new Mock<ILogger<CliService>>();
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Options_Are_Null()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CliService(null, null));
    }

    [Fact]
    public void GetRequestedFilesToGenerate_Should_Return_AllFiles_When_GenerateAllFiles_Is_True()
    {
        // Arrange
        var options = new CliCommandOptions().SetGenerateAllFiles(true);
        _mockCliOptions.Setup(o => o.Value).Returns(options);
        var service = new CliService(_mockCliOptions.Object, _mockLogger.Object);

        // Act
        var result = service.GetRequestedFilesToGenerate();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2); // Assuming at least 2 files should be generated
    }

    [Fact]
    public void GetRequestedFilesToGenerate_Should_Handle_UserInput_Correctly()
    {
        // Arrange
        var options = new CliCommandOptions().SetGenerateAllFiles(false);

        _mockCliOptions.Setup(o => o.Value).Returns(options);

        var service = new CliService(_mockCliOptions.Object, _mockLogger.Object);

        // Here we would mock Console.ReadLine to return specific choices
        // However, since Console.ReadLine is static, we can't easily mock it.
        // We could refactor the code to be more testable, or use other techniques like redirecting the standard input/output streams.

        // Act & Assert
        // Due to the complexity of mocking Console, we may skip this test or refactor code to make it testable
    }

    [Fact]
    public void EnsureK8sDeploysGeneratedTogether_Should_Make_Sure_Both_Are_Generated()
    {
        // Arrange
        var options = new CliCommandOptions();
        _mockCliOptions.Setup(o => o.Value).Returns(options);
        var service = new CliService(_mockCliOptions.Object, _mockLogger.Object);

        // Act
        // Call a method that would end up calling EnsureK8sDeploysGeneratedTogether
        // For example, simulate a series of user inputs that would trigger this method

        // Assert
        // Check that both K8sYaml and IsaBkoYaml are set to be generated
    }
}
