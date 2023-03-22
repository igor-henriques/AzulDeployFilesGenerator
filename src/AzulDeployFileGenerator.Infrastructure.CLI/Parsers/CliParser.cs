namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<CliCommand> ParseArgsAsCommands(string[] args)
    {
        if (!args.Any())
        {
            throw new ApplicationException(Constants.Messages.INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE);
        }

        var triggersIndexes = GetIndexesWhereContainsDashes(args);

        foreach (var triggerIndex in triggersIndexes)
        {
            var argTrigger = args.ElementAt(triggerIndex);

            if (!IsTriggerKnown(argTrigger))
            {
                throw new ApplicationException($"The argument '{argTrigger}' is not valid.");
            }

            var argContent = args.ElementAt(triggerIndex + 1);

            yield return new CliCommand(argContent, argTrigger);
        }
    }

    private static IEnumerable<int> GetIndexesWhereContainsDashes(IEnumerable<string> args)
    {
        return args
            .Select((Trigger, Index) => (Trigger, Index))
            .Where(x => x.Trigger.StartsWith('-') || x.Trigger.StartsWith("--"))
            .Select(x => x.Index);

    }

    private static bool IsTriggerKnown(string trigger)
    {
        return CliCommand.CliCommandTriggers
            .Where(command => command.Value.Contains(trigger))
            .Any();
    }
}
