namespace AzulDeployFileGenerator.Domain.Validators;

/// <summary>
/// Validates the content of <see cref="AppSettings"/>.
/// </summary>
internal sealed class AppSettingsValidator : IValidator<AppSettings>
{
    private readonly List<string> _errors = new();

    private readonly ISolutionFilesService _solutionFilesService;
    private readonly IOptions<CliCommandOptions> _cliOptions;
    private readonly ILogger<AppSettingsValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsValidator"/> class.
    /// </summary>
    /// <param name="solutionFilesService">Service for solution files operations.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cliOptions">CLI command options.</param>
    public AppSettingsValidator(
        ISolutionFilesService solutionFilesService,
        ILogger<AppSettingsValidator> logger,
        IOptions<CliCommandOptions> cliOptions)
    {
        _solutionFilesService = solutionFilesService;
        _logger = logger;
        _cliOptions = cliOptions;
    }

    /// <summary>
    /// Validates the <see cref="AppSettings"/> and throws a <see cref="ValidationException"/> if it fails.
    /// </summary>
    /// <param name="appSettings">The settings to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ValidateAndThrowAsync(AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        await ValidateServiceClients(appSettings.ServiceClients, cancellationToken);
        await ValidateEmptyFields(appSettings, cancellationToken);
        await ValidateEvents(appSettings.Events, cancellationToken);

        if (_cliOptions.Value.ApplicationType is EApplicationType.CronJob && string.IsNullOrEmpty(appSettings.K8sSchedule))
        {
            _errors.Add(Constants.Messages.K8S_SCHEDULE_REQUIRED_ERROR_MESSAGE);
        }

        if (_errors.Any())
        {
            throw new ValidationException(string.Join('\n', _errors));
        }
    }

    /// <summary>
    /// Validates that there are no empty fields in the <see cref="AppSettings"/>.
    /// </summary>
    /// <param name="appSettings">The settings to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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

    /// <summary>
    /// Validates the service clients in the <see cref="AppSettings"/>.
    /// </summary>
    /// <param name="serviceClients">The service clients to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
            _errors.Add(Constants.Messages.SERVICE_CLIENT_DUPLICATES_ERROR_MESSAGE);
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
                _errors.Add(string.Format(Constants.Messages.SERVICE_CLIENT_BAD_SETUP, serviceClient.Id));
            }
            else
            {
                _logger.LogInformation("Service client Id {id} passed the validation\n", serviceClient.Id);
            }
        }
    }

    /// <summary>
    /// Validates the events in the <see cref="AppSettings"/>.
    /// </summary>
    /// <param name="events">The events to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task ValidateEvents(List<Event> events, CancellationToken cancellationToken = default)
    {
        if (events == null)
        {
            if (_cliOptions.Value.ApplicationType is EApplicationType.Consumer)
            {
                _errors.Add(Constants.Messages.NO_APPSETTING_EVENT_CONSUMER_TYPE_ERROR_MESSAGE);
            }

            return;
        }

        const string pattern = @"nameof\(([^)]+)\)";

        foreach (var @event in events)
        {
            _logger.LogInformation("Start validating event Id {Id}\n", @event.Id);

            bool eventValidated = await IsEventValid(@event.Id, pattern, cancellationToken);
            if (!eventValidated)
            {
                _errors.Add(Constants.Messages.NO_APPSETTING_EVENT_CONSUMER_TYPE_ERROR_MESSAGE);
                continue;
            }

            _logger.LogInformation("Event {id} passed the validation\n", @event.Id);
        }
    }

    /// <summary>
    /// Checks if the event is valid.
    /// </summary>
    /// <param name="eventId">The event Id to validate.</param>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the event is valid, otherwise false.</returns>
    private async Task<bool> IsEventValid(string eventId, string pattern, CancellationToken cancellationToken)
    {
        var csharpFiles = await _solutionFilesService.GetCSharpFileWhereContainsText(eventId, cancellationToken: cancellationToken);
        foreach (var csharpFile in csharpFiles)
        {
            if (csharpFile.Contains($"public override string ConnectionId = {eventId};"))
            {
                return true;
            }

            Match match = Regex.Match(csharpFile, pattern);
            if (match.Success && match.Groups[1].Value == eventId)
            {
                return true;
            }
        }

        return false;
    }

}
