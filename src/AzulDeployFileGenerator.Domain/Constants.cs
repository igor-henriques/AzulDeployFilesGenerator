namespace AzulDeployFileGenerator.Domain;

public static class Constants
{
    public static class Messages
    {
        public const string INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE = "You did not provided sufficient arguments to the application.\n\tUse -output to set the output path to the generated files\n\tUse --solution-path to set the path to the solution.";
        public const string FILE_NOT_FOUND_ERROR_MESSAGE = "File {0} were not found in the specified solution path.";
    }

    public static class FileNames 
    {
        public const string AppSettings = "appsettings.json";
    }
}
