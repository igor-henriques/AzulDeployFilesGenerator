namespace AzulDeployFileGenerator.UnitTests.CliTests;

public sealed class CliFileGenerateModelTests
{
    [Fact]
    public void Constructor_Should_Set_Values_Correctly()
    {
        // Arrange & Act
        var model = new CliFileGenerateModel("test.yaml", true);

        // Assert
        model.FileName.Should().Be("test.yaml");
        model.IsToGenerate.Should().BeTrue();
    }

    [Fact]
    public void SetIsToGenerate_Should_Update_Value()
    {
        // Arrange
        var model = new CliFileGenerateModel("test.yaml", true);

        // Act
        model.SetIsToGenerate(false);

        // Assert
        model.IsToGenerate.Should().BeFalse();
    }

    [Theory]
    [InlineData(Constants.FileNames.K8sYaml, true)]
    [InlineData(Constants.FileNames.IsaBkoYaml, true)]
    [InlineData("random.yaml", false)]
    public void RequiresDeployName_Should_Return_Expected_Value(string fileName, bool expected)
    {
        // Arrange
        var model = new CliFileGenerateModel(fileName, true);

        // Act & Assert
        model.RequiresDeployName().Should().Be(expected);
    }

    [Fact]
    public void ImplicitOperator_Should_Set_Fields_Correctly()
    {
        // Arrange & Act
        CliFileGenerateModel model = "test.yaml";

        // Assert
        model.FileName.Should().Be("test.yaml");
        model.IsToGenerate.Should().BeTrue();
    }
}
