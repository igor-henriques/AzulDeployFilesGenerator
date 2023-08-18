using AzulDeployFileGenerator.Domain.Models.AppSettingsObjects;
using AzulDeployFileGenerator.Domain.Models.Cli;

namespace AzulDeployFileGenerator.Domain.Validators;

public sealed class AppSettingsValidator : IValidator<AppSettings>
{
    private readonly List<string> _errors = new();
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly ILogger<AppSettingsValidator> _logger;

    public AppSettingsValidator(
        ISolutionFilesService solutionFilesService,
        IOptions<CliCommandOptions> cliOptions,
        ILogger<AppSettingsValidator> logger)
    {
        _solutionFilesService = solutionFilesService;
        _cliOptions = cliOptions;
        _logger = logger;
    }

    public async Task ValidateAndThrowAsync(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        await ValidateServiceClients(appSettings.ServiceClients, cancellationToken);
        await ValidateEmptyFields(appSettings, cancellationToken);

        if (_errors.Any())
        {
            throw new ValidationException(string.Join('\n', _errors));
        }
    }

    private async Task ValidateEmptyFields(object appSettings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = appSettings.GetType();
        foreach (var props in type.GetProperties())
        {
            var value = props.GetValue(appSettings);
            if (value is string or null && string.IsNullOrWhiteSpace(value?.ToString())
                || (props.PropertyType.IsValueType && value.GetType() != typeof(bool) && Activator.CreateInstance(props.PropertyType).Equals(value)))
            {
                _errors.Add($"Property {props.Name} is null or empty.");
            }
            else if (props.PropertyType.IsClass
                && !typeof(IEnumerable<object>).IsAssignableFrom(props.PropertyType)
                && !value.GetType().Equals(typeof(string)))
            {
                await ValidateEmptyFields(value, cancellationToken);
            }
            else if (value is IEnumerable<object> lista)
            {
                foreach (var item in lista)
                {
                    await ValidateEmptyFields(item, cancellationToken);
                }
            }
        }
    }

    private async Task ValidateServiceClients(List<ServiceClient> serviceClients, CancellationToken cancellationToken = default)
    {
        var distinctServicesClients = serviceClients.DistinctBy(s => s.Id);
        var hasDistinctIds = distinctServicesClients.Count() == serviceClients.Count;

        if (!hasDistinctIds)
        {
            _errors.Add("Bad ServiceClient setup. Duplicates found.");
            return;
        }

        foreach (var serviceClient in distinctServicesClients)
        {
            _logger.LogInformation("Start validating service client Id {id}", serviceClient.Id);

            bool isServiceClientValid = await _solutionFilesService.AnyClassContainsString(
                _cliOptions.Value.SolutionPath,
                serviceClient.FormattedIdForSearchingInClasses,
                cancellationToken);

            if (!isServiceClientValid)
            {
                _errors.Add($"Bad setup for ServiceClient Id {serviceClient.Id}");
            }
            else
            {
                _logger.LogInformation("Service client Id {id} passed the validation", serviceClient.Id);
            }
        }
    }
}
