namespace AzulDeployFileGenerator.CLI;

internal static class Constants
{
    internal const string OUTPUT_COMMAND_TYPE = "output";

    internal static Dictionary<string, string[]> CommandTriggers = new Dictionary<string, string[]>()
    {
        { OUTPUT_COMMAND_TYPE, new[] { "-o", "-output"} }
    };
}
