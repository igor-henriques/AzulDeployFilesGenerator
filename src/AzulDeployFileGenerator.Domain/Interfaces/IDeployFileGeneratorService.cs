namespace AzulDeployFileGenerator.Domain.Interfaces;

public interface IDeployFileGeneratorService
{
    /// <summary>
    /// Generates the appsettings.json file for the online environment.
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAppSettingsOnline(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a tokenized version of the appsettings, stands for appsettings.Docker.json
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAppSettingsDocker(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates k8sdeploy.yaml file
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAzulK8sDeploy(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates isabkodeploy.yaml file
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateOnlineK8sDeploy(AppSettings appSettings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates Dockerfile for Azul environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateAzulDockerfile(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates Dockerfile for Online environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateOnlineDockerfile(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a tokenized Excel version of the appsettings, following Azul Airlines guidelines
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task GenerateTokenizationExcelSheet(AppSettings appSettings, CancellationToken cancellationToken = default);
}
