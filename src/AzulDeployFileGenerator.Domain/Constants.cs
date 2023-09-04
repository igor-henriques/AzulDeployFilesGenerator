namespace AzulDeployFileGenerator.Domain;

public static class Constants
{
    public const string PUBLISHER_FILE_IDENTIFIER = ": EventPublisher<";
    public const string SUBSCRIBER_FILE_IDENTIFIER = ": EventSubscriber<";
    public const string DEFAULT_ONLINE_KUBERNETES_NAMESPACE = "deploy-generator";
    public const string DEFAULT_QUIT_CMD_FILES_SELECTION_CHAR = "q";
    public const int DEFAULT_EXPOSED_API_PORT_DOCKERFILE = 80;

    public static class ExcelDefaults
    {
        public const string DEFAULT_FONT_NAME = "Calibri";
        public readonly static Color DEFAULT_MENU_BACKGROUND_COLOR = Color.DarkBlue;
        public readonly static Color DEFAULT_GREEN_COLOR_TONE = Color.LimeGreen;
        public readonly static Color DEFAULT_YELLOW_COLOR_TONE = Color.Yellow;
        public readonly static Color DEFAULT_RED_COLOR_TONE = Color.Red;
        public readonly static Color DEFAULT_ORANGE_COLOR_TONE = Color.Orange;
        public const int MAX_COLUMN_WIDTH = 75;
    }

    public static class Messages
    {
        public const string INSUFFICIENT_ARGUMENTS_WARNING_MESSAGE = "As you did not provided sufficient arguments to the application on startup, you should provide it next:\n\n";
        public const string FILE_NOT_FOUND_ERROR_MESSAGE = "File {0} were not found in the specified solution path. Check and try again.";
        public const string INVALID_FILE_NAME_ERROR_MESSAGE = "File {0} is not assigned as a generable file. Check and try again.";
        public const string INVALID_ARG_TRIGGER_ERROR_MESSAGE = "The argument '{0}' is not valid. Check and try again.";
        public const string INVALID_PATH_ERROR_MESSAGE = "Directory '{0}' is not a valid a path. Check and try again.";
        public const string INVALID_APP_TYPE_ERROR_MESSAGE = "App type {0} isn't valid. Supported app types: {1}. Check and try again.";
        public const string INVALID_INPUT_ERROR_MESSAGE = "Invalid input. Press any key to continue...\n";
        public const string INVALID_APPLICATION_TYPE_ERROR_MESSAGE = "{0} doesn't match any valid application type. Check and try again.";
        public const string INVALID_IMAGE_NAME_ERROR_MESSAGE = "{0} is not a valid image name. Wrong ACR name provided. Check and try again.";
        public const string INVALID_DEPLOY_NAME_ERROR_MESSAGE = "Deploy name '{0}' isn't valid. Check Azul documentation to get the recommendations. Check and try again.";
        public const string FILES_TO_GENERATE_INFO_MESSAGE = "\nIndicate the relative number to set as false/true the files to generate, or 'q' to leave:";
        public const string GLOBAL_EXCEPTION_HANDLER_ERROR_MESSAGE = "{exception}\n\nPress any key to exit.";
        public const string GET_OUTPUT_PATH_MESSAGE = "Provide a output path:";
        public const string GET_SOLUTION_PATH_MESSAGE = "Provide a solution path:";
        public const string GET_AN_APPLICATION_TYPE_MESSAGE = "Provide an application type [api/consumer/cronjob]:";
        public const string GET_DEPLOY_NAME_MESSAGE = "Provide a deploy name:";
        public const string GET_IMAGE_NAME_MESSAGE = "Provide a image name:";
        public const string NO_ACTIONS_MESSAGE = "No file selected to generate. Finishing the application without any actions.";
        public const string MORE_THAN_ONE_FILE_FOUND_ERROR_MESSAGE = "Only one {0} is allowed. Check and try again.";
        public const string MORE_THAN_ONE_ASSEMBLY_FOUND_ERROR_MESSAGE = "Only one csproj file is allowed in the same directory. Check and try again.";
        public const string EXECUTING_DOTNET_CLEAN_MESSAGE = "Executing 'dotnet clean'.\n";
        public const string SOLUTION_CLEANED = "Solution successfully cleaned.\n";
        public const string DEFAULT_IMAGE_TOKENIZER_DESCRIPTION = "Serviço demo para tokenização de aplicativos backend cloud";
        public const string NO_PUBLISHERS_OR_SUBSCRIBERS_ERROR_MESSAGE = "The application type were set as Cronjob/Consumer, but no publishers or subscribers were found in the solution. Check and try again.";
        public const string NO_APPSETTING_EVENT_CONSUMER_TYPE_ERROR_MESSAGE = "As a Consumer, the appsettings must have at least one 'event' defined. Check and try again.";
        public const string SERVICE_CLIENT_DUPLICATES_ERROR_MESSAGE = "Bad ServiceClient setup. Duplicates found. Check and try again.\n";
        public const string SERVICE_CLIENT_BAD_SETUP = "Bad setup for ServiceClient Id {0}\n";
        public const string SEARCHING_FILE_NAME_INFO_MESSAGE = "Searching for {fileName}\n";
        public const string FILE_FOUND_INFO_MESSAGE = "{fileName} found\n";
        public const string FILE_SUCCESSFULLY_GENERATED_INFO_MESSAGE = "'{fileName}' successfully generated at {outputPath}\n\n";
        public const string EXCEPTION_GENERATING_FILE_WITH_MULTIPLES_REQUIRED_ERROR_MESSAGE = "Error generating {fileName}. Exception: {exception}\nMoving to the next.";
        public const string EXCEPTION_GENERATING_SINGLE_FILE_WITH_ERROR_MESSAGE = "Error generating {fileName}. Exception: {exception}\nNo more files to generate.";
        public const string NUGET_CONFIG_KEY_NOT_FOUND_ERROR_MESSAGE = "AzulFramework key not found in the nugetconfig file. Check and try again";
        public const string NUGET_CONFIG_KEY_NOT_DEFINED_ERROR_MESSAGE = "AzulFramework key not defined in the appsettings.json. Check and try again";
        public const string K8S_SCHEDULE_REQUIRED_ERROR_MESSAGE = "As a cron job, the appsettings.json must have a 'k8s.schedule' property defined in the root. Check and try again.";
    }

    public static class FileNames
    {
        public const string AppSettings = "appsettings.json";
        public const string AppSettingsOnline = "appsettings.Online.json";
        public const string AppSettingsDocker = "appsettings.Docker.json";
        public const string K8sYaml = "k8sdeploy.yaml";
        public const string IsaBkoYaml = "isabkodeploy.yaml";
        public const string DeploySheet = "{0}.xlsx"; // Format for getting the project name from the cli options
        public const string Dockerfile = "Dockerfile";
        public const string DockerfileOnline = "DockerfileOnline";
        public const string NugetConfig = "nuget.config";
        public const string Program = "Program.cs";
    }

    public static class ImageNames
    {
        public readonly static string BASE_AZUL_ACR_NAME = SettingsAccessor.Instance.AzulAcrName;
        public readonly static string BASE_ONLINE_ACR_NAME = SettingsAccessor.Instance.OnlineAcrName;

        public const string AZUL_DOTNET_CORE_SDK = "acrdevopsbr.azurecr.io/custom/dotnet/core/sdk:3.1.405";
        public const string AZUL_ASPNET_CORE = "acrdevopsbr.azurecr.io/custom/dotnet/core/aspnet:3.1.11";
        public const string ISABKO_DOTNET_CORE_SDK = "isabko.azurecr.io/dotnet/core/sdk:3.1.405";
        public const string ISABKO_ASPNET_CORE = "isabko.azurecr.io/dotnet/core/aspnet:3.1.11";
    }
}
