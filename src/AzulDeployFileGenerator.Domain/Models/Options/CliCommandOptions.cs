namespace AzulDeployFileGenerator.Domain.Models.Options;

public sealed record CliCommandOptions
{
    public string SolutionPath { get; private set; }
    public string OutputPath { get; private set; }
    public EApplicationType ApplicationType { get; private set; }
    public string ApplicationName { get; private set; }
    public string DeployName { get; private set; }
    public string ImageName { get; private set; }
    public string IsaBkoImageName => ImageName.Replace(Constants.ImageNames.BASE_AZUL_ACR_NAME, Constants.ImageNames.BASE_ONLINE_ACR_NAME);

    public CliCommandOptions SetSolutionPath(string solutionPath)
    {
        SolutionPath = solutionPath;
        return this;
    }

    public CliCommandOptions SetOutputPath(string outputPath)
    {
        OutputPath = outputPath;
        return this;
    }

    public CliCommandOptions SetApplicationType(string applicationType)
    {
        if (!Enum.TryParse<EApplicationType>(applicationType, ignoreCase: true, out var appType))
        {
            throw new InvalidCastException(string.Format(Constants.Messages.INVALID_APPLICATION_TYPE_ERROR_MESSAGE, applicationType));
        }

        ApplicationType = appType;
        return this;
    }

    public CliCommandOptions SetApplicationName(string applicationName)
    {
        ApplicationName = applicationName;
        return this;
    }

    public CliCommandOptions SetDeployName(string deployName)
    {
        if (string.IsNullOrWhiteSpace(deployName) || !deployName.Contains('-'))
        {
            throw new InvalidOperationException(string.Format(Constants.Messages.INVALID_DEPLOY_NAME_ERROR_MESSAGE, deployName));
        }

        DeployName = deployName;
        return this;
    }

    public CliCommandOptions SetImageName(string imageName)
    {
        if (!imageName.Contains(Constants.ImageNames.BASE_AZUL_ACR_NAME) && !imageName.Contains(Constants.ImageNames.BASE_ONLINE_ACR_NAME))
        {
            throw new InvalidOperationException(string.Format(Constants.Messages.INVALID_IMAGE_NAME_ERROR_MESSAGE, imageName));
        }

        ImageName = imageName;
        return this;
    }
}
