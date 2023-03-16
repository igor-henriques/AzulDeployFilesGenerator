using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<Command> ParseArgsAsCommands(string[] args)
    {
        var triggersIndexes = GetIndexesWhereContainsDashes(args);

        foreach (var triggerIndex in triggersIndexes)
        {
            var argTrigger = args.ElementAt(triggerIndex);

            if (!IsTriggerKnown(argTrigger))
            {
                throw new ApplicationException($"The argument '{argTrigger}' is not valid. Try -help for commands guidelines.");
            }

            var argContent = args.ElementAt(triggerIndex + 1);

            yield return new Command(argContent, argTrigger);
        }
    }

    private static IEnumerable<int> GetIndexesWhereContainsDashes(IEnumerable<string> args)
    {
        return args.Where(a => a.Contains('-')).Select((_, index) => index);
    }    

    private static bool IsTriggerKnown(string trigger)
    {
        return Constants.CommandTriggers.Where(x => x.Value.Contains(trigger)).Any();
    }
}
