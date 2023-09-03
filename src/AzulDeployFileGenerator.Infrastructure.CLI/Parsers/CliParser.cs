namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<CliCommand> ParseArgsAsCommands(string[] args)
    {
        if (!args.Any(CliCommand.IsAnyOfRequiredTriggers))
        {
            throw new ApplicationException(Constants.Messages.INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE);
        }

        var triggersIndexes = GetIndexesWhereContainsDashes(args);

        foreach (var triggerIndex in triggersIndexes)
        {
            var argTrigger = args.ElementAt(triggerIndex);

            if (!CliCommand.IsValidCommandTrigger(argTrigger))
            {
                throw new ApplicationException(string.Format(Constants.Messages.INVALID_ARG_TRIGGER_ERROR_MESSAGE, argTrigger));
            }

            var argContent = args.Length > triggerIndex + 1 ? args.ElementAt(triggerIndex + 1) : string.Empty;

            yield return new CliCommand(argContent, argTrigger);
        }
    }

    /// <summary>
    /// Useful to get the indexes of the args that contains dashes, which are the triggers for the commands
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static IEnumerable<int> GetIndexesWhereContainsDashes(IEnumerable<string> args)
    {
        return args
            .Select((Trigger, Index) => (Trigger, Index))
            .Where(x => x.Trigger.StartsWith('-') || x.Trigger.StartsWith("--"))
            .Select(x => x.Index);
    }
}
