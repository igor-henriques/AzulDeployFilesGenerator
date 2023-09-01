namespace AzulDeployFileGenerator.Domain.Models.Cli;

public sealed record CliCommand
{
    public const string OUTPUT_PATH_COMMAND_ID = "output";
    public const string SOLUTION_PATH_COMMAND_ID = "solution-path";
    public const string APP_TYPE_COMMAND_ID = "app-type";
    public const string HELP_COMMAND_ID = "help";
    public const string DEPLOY_NAME_COMMAND_ID = "deploy-name";
    public const string IMAGE_NAME_COMMAND_ID = "image-name";

    public CliCommand(string content, string trigger)
    {
        Content = content;
        Trigger = trigger;

        Validate();
    }

    public string Content { get; private set; }
    public string Trigger { get; init; }
    public bool IsOutputPathCommandType
    {
        get => CliCommandTriggers
            .Where(command => command.Value.Contains(Trigger) && command.Key.Equals(OUTPUT_PATH_COMMAND_ID))
            .Any();
    }

    public bool IsSolutionPathCommandType
    {
        get => CliCommandTriggers
            .Where(command => command.Value.Contains(Trigger) && command.Key.Equals(SOLUTION_PATH_COMMAND_ID))
            .Any();
    }

    public bool IsAppTypeCommandType
    {
        get => CliCommandTriggers
            .Where(command => command.Value.Contains(Trigger) && command.Key.Equals(APP_TYPE_COMMAND_ID))
            .Any();
    }

    public bool IsDeployNameCommandType
    {
        get => CliCommandTriggers
           .Where(command => command.Value.Contains(Trigger) && command.Key.Equals(DEPLOY_NAME_COMMAND_ID))
           .Any();
    }

    public bool IsImageNameCommandType
    {
        get => CliCommandTriggers
           .Where(command => command.Value.Contains(Trigger) && command.Key.Equals(IMAGE_NAME_COMMAND_ID))
           .Any();
    }

    private void Validate()
    {
        if (IsOutputPathCommandType || IsSolutionPathCommandType)
        {
            if (Content is ".")
            {
                Content = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(Content))
            {
                throw new InvalidOperationException(string.Format(Constants.Messages.INVALID_PATH_ERROR_MESSAGE, Content));
            }
        }

        if (IsAppTypeCommandType && !DefaultAppTypes.Contains(Content, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(string.Format(Constants.Messages.INVALID_APP_TYPE_ERROR_MESSAGE, Content, string.Join(",", DefaultAppTypes)));
        }
    }

    public static readonly IReadOnlyDictionary<string, string[]> CliCommandTriggers = new Dictionary<string, string[]>()
    {
        { "output", new[] { "-output", "-o" } },
        { "solution-path", new[] { "--solution-path", "-sp"} },
        { "app-type", new[] { "--app-type", "-at"} },
        { "deploy-name", new[] { "--deploy-name", "-dn" } },
        { "image-name", new[] { "--image-name", "-in" } },
    };

    public static readonly IReadOnlyList<string> DefaultAppTypes = new List<string>()
    {
        "api",
        "consumer",
        "cronjob"
    };

    public static string GetHelpCommands()
    {
        StringBuilder sb = new();

        sb.AppendLine($"[Required] Setting the output path to the generated files: {string.Join(" or ", CliCommandTriggers[OUTPUT_PATH_COMMAND_ID])}");
        sb.AppendLine($"[Required] Setting the solution path which the generator will work on: {string.Join(" or ", CliCommandTriggers[SOLUTION_PATH_COMMAND_ID])}");
        sb.AppendLine($"[Required] Setting the app type so the generator creates the right deploy files: {string.Join(" or ", CliCommandTriggers[APP_TYPE_COMMAND_ID])} [{string.Join("/", DefaultAppTypes)}]");
        sb.AppendLine($"[Optional] Setting the deploy name so the generator be able to create the .yaml files correctly: {string.Join(" or ", CliCommandTriggers[DEPLOY_NAME_COMMAND_ID])}");
        sb.AppendLine($"[Optional] Setting the image name so the generator be able to create the .yaml files correctly: {string.Join(" or ", CliCommandTriggers[IMAGE_NAME_COMMAND_ID])}");

        return sb.ToString();
    }

    public static bool IsValidCommandTrigger(string trigger)
    {
        return CliCommandTriggers.Any(command => command.Value.Contains(trigger));
    }
}