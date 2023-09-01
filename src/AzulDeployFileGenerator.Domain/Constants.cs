﻿namespace AzulDeployFileGenerator.Domain;

public static class Constants
{
    public static class Messages
    {
        public const string INSUFFICIENT_ARGUMENTS_ERROR_MESSAGE = "You did not provided sufficient arguments to the application.\n\tUse -help to get the required arguments";
        public const string FILE_NOT_FOUND_ERROR_MESSAGE = "File {0} were not found in the specified solution path.";
        public const string INVALID_ARG_TRIGGER_ERROR_MESSAGE = "The argument '{0}' is not valid.";
        public const string INVALID_PATH_ERROR_MESSAGE = "Directory '{0}' is not a valid a path.";
        public const string INVALID_APP_TYPE_ERROR_MESSAGE = "App type {0} isn't valid. Supported app types: {1)}";
        public const string INVALID_INPUT_ERROR_MESSAGE = "Invalid input. Press any key to continue...";
        public const string INVALID_APPLICATION_TYPE_ERROR_MESSAGE = "{0} doesn't match any valid application type";
        public const string INVALID_IMAGE_NAME_ERROR_MESSAGE = "{0} is not a valid image name. Wrong ACR name provided.";
        public const string INVALID_DEPLOY_NAME_ERROR_MESSAGE = "Deploy name '{0}' isn't valid. Check Azul documentation to get the recommendations.";
        public const string FILES_TO_GENERATE_INFO_MESSAGE = "\nIndicate the relative number to set as false/true the files to generate, or 'q' to leave:";
        public const string GLOBAL_EXCEPTION_HANDLER_ERROR_MESSAGE = "{exception}\n\nPress any key to exit.";
        public const string GET_DEPLOY_NAME_MESSAGE = "Provide a deploy name:";
        public const string GET_IMAGE_NAME_MESSAGE = "Provide a image name:";
        public const string NO_ACTIONS_MESSAGE = "No file selected to generate. Finishing the application without any actions.";
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
    }

    public const string BASE_AZUL_ACR_NAME = "acrdevopsbr";
    public const string BASE_ONLINE_ACR_NAME = "isabko";
}
