namespace AzulDeployFileGenerator.UnitTests.CliTests;

public sealed class CliParserTests
{
    [Fact]
    public void ParseArgsAsCommands_Should_ThrowException_When_NoArgsArePassed()
    {
        // Arrange
        var args = new string[] { };

        // Act & Assert
        Assert.Throws<ApplicationException>(() => CliParser.ParseArgsAsCommands(args).ToList());
    }

    [Fact]
    public void ParseArgsAsCommands_Should_ThrowException_When_ArgTriggerIsUnknown()
    {
        // Arrange
        var args = new string[] { "-u", "username" };

        // Act & Assert
        Assert.Throws<ApplicationException>(() => CliParser.ParseArgsAsCommands(args).ToList());
    }

    [Fact]
    public void ParseArgsAsCommands_Should_ReturnCliCommand_When_ValidArgsArePassed()
    {
        // Arrange
        var currentDirectory = Directory.GetCurrentDirectory();
        var args = new string[] { "-o", currentDirectory };

        // Act
        var result = CliParser.ParseArgsAsCommands(args).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(args[1], result[0].Content);
        Assert.Equal(args[0], result[0].Trigger);
    }

    [Fact]
    public void ParseArgsAsCommands_Should_ReturnCliCommand_When_ValidArgsArePassedWithDotAsPath()
    {
        // Arrange
        var currentDirectoryAsDot = ".";
        var currentDirectory = Directory.GetCurrentDirectory();
        var args = new string[] { "-o", currentDirectoryAsDot };

        // Act
        var result = CliParser.ParseArgsAsCommands(args).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(currentDirectory, result[0].Content);
        Assert.Equal(args[0], result[0].Trigger);
    }
}
