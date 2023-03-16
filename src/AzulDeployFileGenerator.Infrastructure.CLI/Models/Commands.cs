namespace AzulDeployFileGenerator.CLI.Models;

public sealed record Command
{
    public Command(string content, string trigger)
    {
        Content = content;
        Trigger = trigger;

        Validate();
    }

    public string Content { get; init; }
    public string Trigger { get; init; }

    private void Validate()
    {
        if (IsOutputCommandType)
        {
            if (!Directory.Exists(Content))
            {
                throw new InvalidOperationException($"Directory '{Content}' is not a valid a path");
            }
        }
    }

    public bool IsOutputCommandType
    {
        get => Constants.CommandTriggers.Where(x => x.Value.Contains(Trigger)).Any();
    }
}