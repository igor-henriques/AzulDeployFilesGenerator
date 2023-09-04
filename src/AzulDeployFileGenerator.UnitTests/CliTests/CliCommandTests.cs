namespace AzulDeployFileGenerator.UnitTests.CliTests;

public sealed class CliCommandTests
{
    [Theory]
    [InlineData("-output", ".")]
    [InlineData("-o", ".")]
    public void Should_Set_Content_And_Trigger_For_Valid_OutputPath_Command(string trigger, string content)
    {
        // Arrange & Act
        var cliCommand = new CliCommand(content, trigger);

        // Assert
        cliCommand.Trigger.Should().Be(trigger);
        cliCommand.Content.Should().Be(Directory.GetCurrentDirectory());
    }

    [Fact]
    public void Should_Throw_Exception_For_Invalid_OutputPath_Command()
    {
        // Arrange
        string trigger = "-output";
        string invalidContent = "InvalidPath";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new CliCommand(invalidContent, trigger));
    }

    [Theory]
    [InlineData("--solution-path", ".")]
    [InlineData("-sp", ".")]
    public void Should_Set_Content_And_Trigger_For_Valid_SolutionPath_Command(string trigger, string content)
    {
        // Arrange & Act
        var cliCommand = new CliCommand(content, trigger);

        // Assert
        cliCommand.Trigger.Should().Be(trigger);
        cliCommand.Content.Should().Be(Directory.GetCurrentDirectory());
    }

    [Theory]
    [InlineData("-output", true)]
    [InlineData("-o", true)]
    [InlineData("-unknown", false)]
    public void IsOutputPathCommandType_Should_Return_Expected_Result(string trigger, bool expected)
    {
        // Act & Assert
        CliCommand.IsOutputPathCommandType(trigger).Should().Be(expected);
    }

    [Theory]
    [InlineData("--solution-path", true)]
    [InlineData("-sp", true)]
    [InlineData("-unknown", false)]
    public void IsSolutionPathCommandType_Should_Return_Expected_Result(string trigger, bool expected)
    {
        // Act & Assert
        CliCommand.IsSolutionPathCommandType(trigger).Should().Be(expected);
    }

    [Theory]
    [InlineData("--app-type", "api")]
    [InlineData("--application-type", "consumer")]
    [InlineData("-at", "cronjob")]
    public void Should_Set_Content_And_Trigger_For_Valid_AppType_Command(string trigger, string content)
    {
        // Arrange & Act
        var cliCommand = new CliCommand(content, trigger);

        // Assert
        cliCommand.Trigger.Should().Be(trigger);
        cliCommand.Content.Should().Be(content);
    }

    [Fact]
    public void Should_Throw_Exception_For_Invalid_AppType_Command()
    {
        // Arrange
        string trigger = "--app-type";
        string invalidContent = "InvalidAppType";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new CliCommand(invalidContent, trigger));
    }

    [Theory]
    [InlineData("--app-type", true)]
    [InlineData("--application-type", true)]
    [InlineData("-at", true)]
    [InlineData("-unknown", false)]
    public void IsAppTypeCommandType_Should_Return_Expected_Result(string trigger, bool expected)
    {
        // Act & Assert
        CliCommand.IsAppTypeCommandType(trigger).Should().Be(expected);
    }

    [Theory]
    [InlineData("-output", true)]
    [InlineData("--solution-path", true)]
    [InlineData("--app-type", true)]
    [InlineData("-unknown", false)]
    public void IsAnyOfRequiredTriggers_Should_Return_Expected_Result(string trigger, bool expected)
    {
        // Act & Assert
        CliCommand.IsAnyOfRequiredTriggers(trigger).Should().Be(expected);
    }

    [Theory]
    [InlineData("-output", true)]
    [InlineData("--unknown-command", false)]
    public void IsValidCommandTrigger_Should_Return_Expected_Result(string trigger, bool expected)
    {
        // Act & Assert
        CliCommand.IsValidCommandTrigger(trigger).Should().Be(expected);
    }
}
