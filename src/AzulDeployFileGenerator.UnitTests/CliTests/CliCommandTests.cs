namespace AzulDeployFileGenerator.UnitTests.CliTests;

public sealed class CliCommandTests
{
    [Theory]
    [InlineData(".", "-output", true)]
    [InlineData("C:/Test/Output", "-output", true)]
    [InlineData(".", "--solution-path", false)]
    [InlineData("C:/Test/Solution", "--solution-path", false)]
    public void IsOutputPathCommandType_ShouldReturnCorrectValue(string content, string trigger, bool expected)
    {
        // Arrange
        if (content is not "." && !Directory.Exists(content))
        {
            Directory.CreateDirectory(content);
        }

        var command = new CliCommand(content, trigger);

        // Act
        var result = CliCommand.IsOutputPathCommandType(command.Trigger);

        // Assert
        Assert.Equal(expected, result);

        if (content is not ".")
        {
            Directory.Delete(content);
        }
    }

    [Theory]
    [InlineData(".", "--solution-path", true)]
    [InlineData("C:/Test/Solution", "--solution-path", true)]
    [InlineData(".", "-output", false)]
    [InlineData("C:/Test/Output", "-output", false)]
    public void IsSolutionPathCommandType_ShouldReturnCorrectValue(string content, string trigger, bool expected)
    {
        // Arrange
        if (content is not "." && !Directory.Exists(content))
        {
            Directory.CreateDirectory(content);
        }

        var command = new CliCommand(content, trigger);

        // Act
        var result = CliCommand.IsSolutionPathCommandType(command.Trigger);

        // Assert
        Assert.Equal(expected, result);

        if (content is not ".")
        {
            Directory.Delete(content);
        }
    }

    [Fact]
    public void CliCommand_WithInvalidDirectory_ShouldThrowException()
    {
        // Arrange
        var content = "InvalidDirectory";
        var trigger = "-output";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new CliCommand(content, trigger));
    }

    [Fact]
    public void CliCommand_WithValidDirectory_ShouldNotThrowException()
    {
        // Arrange
        var content = Directory.GetCurrentDirectory();
        var trigger = "-output";

        // Act & Assert
        Assert.Null(Record.Exception(() => new CliCommand(content, trigger)));
    }
}
