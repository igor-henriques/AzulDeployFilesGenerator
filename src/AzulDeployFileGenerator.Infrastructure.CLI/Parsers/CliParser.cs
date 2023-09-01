namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<CliCommand> ParseArgsAsCommands(string[] args)
    {
        if (args.Any(a => a.Contains(CliCommand.HELP_COMMAND_ID)))
        {
            Console.Out.WriteLine(CliCommand.GetHelpCommands());
            Environment.Exit(0);
        }

        if (!args.Any(a =>
        {
            return CliCommand.CliCommandTriggers[CliCommand.OUTPUT_PATH_COMMAND_ID].Contains(a) 
            || CliCommand.CliCommandTriggers[CliCommand.SOLUTION_PATH_COMMAND_ID].Contains(a) 
            || CliCommand.CliCommandTriggers[CliCommand.APP_TYPE_COMMAND_ID].Contains(a);
        }))
        {
            throw new ApplicationException(Constants.Messages.INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE);
        }
        
        var triggersIndexes = GetIndexesWhereContainsDashes(args);

        foreach (var triggerIndex in triggersIndexes)
        {
            var argTrigger = args.ElementAt(triggerIndex);

            if (!IsTriggerKnown(argTrigger))
            {
                throw new ApplicationException(string.Format(Constants.Messages.INVALID_ARG_TRIGGER_ERROR_MESSAGE, argTrigger));
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
            .Any(command => command.Value.Contains(trigger));            
    }
}
