namespace AzulDeployFileGenerator.Infrastructure.CLI.Models;

public sealed record CliCommand
{
    private const string OUTPUT_PATH_COMMAND_ID = "output";
    private const string SOLUTION_PATH_COMMAND_ID = "solution-path";

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
                throw new InvalidOperationException($"Directory '{Content}' is not a valid a path");
            }
        }
    }

    public static readonly IReadOnlyDictionary<string, string[]> CliCommandTriggers = new Dictionary<string, string[]>()
    {
        { "output", new[] { "-o", "-output"} },
        { "solution-path", new[] { "--solution-path"} },
    };
}