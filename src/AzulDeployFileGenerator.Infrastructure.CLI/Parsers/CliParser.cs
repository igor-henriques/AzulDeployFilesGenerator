namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<CliCommand> ParseArgsAsCommands(string[] args)
    {        
        if (!args.Any(arg =>
        {
            return CliCommand.CliCommandTriggers[CliCommand.OUTPUT_PATH_COMMAND_ID].Contains(arg) 
                || CliCommand.CliCommandTriggers[CliCommand.SOLUTION_PATH_COMMAND_ID].Contains(arg) 
                || CliCommand.CliCommandTriggers[CliCommand.APP_TYPE_COMMAND_ID].Contains(arg);
        }))
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

            var argContent = args.ElementAt(triggerIndex + 1);

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
