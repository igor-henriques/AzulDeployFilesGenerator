namespace AzulDeployFileGenerator.CLI.Parsers;

public static class CliParser
{
    public static IEnumerable<Command> ParseArgsAsCommands(string[] args)
    {
        foreach (var arg in args)
        {
            yield return new Command();
        }
    }
}
