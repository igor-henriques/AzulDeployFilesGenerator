﻿namespace AzulDeployFileGenerator.Domain.Validators;

internal sealed class AppSettingsValidator : IValidator<AppSettings>
{
    private readonly List<string> _errors = new();
    private readonly ISolutionFilesService _solutionFilesService;
    private readonly ILogger<AppSettingsValidator> _logger;

    public AppSettingsValidator(
        ISolutionFilesService solutionFilesService,
        ILogger<AppSettingsValidator> logger)
    {
        _solutionFilesService = solutionFilesService;
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
        foreach (var prop in type.GetProperties())
        {          
            if (prop.Name.Contains(AppSettings.EXTRA_PROPERTIES_NAME))
            {
                continue;
            }

            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = prop.GetValue(appSettings);

            var jsonPropertyAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute != null 
                && jsonPropertyAttribute.NullValueHandling == NullValueHandling.Ignore
                && value is string or null
                && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                continue;
            }                
            
            if (value is string or null && string.IsNullOrWhiteSpace(value?.ToString())
                || (prop.PropertyType.IsValueType && value.GetType() != typeof(bool) && Activator.CreateInstance(prop.PropertyType).Equals(value)))
            {
                _errors.Add($"Property {prop.Name} is null or empty.");
            }
            else if (prop.PropertyType.IsClass
                && !typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType)
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
        if (serviceClients is null || serviceClients?.Count is 0)
        {
            return;
        }

        var distinctServicesClients = serviceClients.DistinctBy(s => s.Id);
        var hasDistinctIds = distinctServicesClients.Count() == serviceClients.Count;

        if (!hasDistinctIds)
        {
            _errors.Add("Bad ServiceClient setup. Duplicates found.\n");
            return;
        }

        foreach (var serviceClient in distinctServicesClients)
        {
            _logger.LogInformation("Start validating service client Id {id}\n", serviceClient.Id);

            bool isServiceClientValid = await _solutionFilesService.ContainsTextInAnyCSharpFileAsync(
                serviceClient.FormattedIdForSearchingInSolutionClasses,
                cancellationToken: cancellationToken);

            if (!isServiceClientValid)
            {
                _errors.Add($"Bad setup for ServiceClient Id {serviceClient.Id}\n");
            }
            else
            {
                _logger.LogInformation("Service client Id {id} passed the validation\n", serviceClient.Id);
            }
        }
    }
}
