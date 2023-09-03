﻿using System.Runtime.CompilerServices;
using Console = System.Console;

namespace AzulDeployFileGenerator.Infrastructure.CLI;

internal sealed class CliService : ICliService
{
    private readonly IOptions<CliCommandOptions> _cliOptions;

    public CliService(IOptions<CliCommandOptions> cliOptions)
    {
        _cliOptions = cliOptions ?? throw new ArgumentNullException(nameof(cliOptions)); ;
    }

    private readonly List<CliFileGenerateModel> _files = new()
    {
        new(fileName: Constants.FileNames.AppSettingsDocker, isToGenerate: false),
        new(fileName: Constants.FileNames.AppSettingsOnline, isToGenerate: false),
        new(fileName: Constants.FileNames.K8sYaml, isToGenerate: false),
        new(fileName: Constants.FileNames.IsaBkoYaml, isToGenerate: false),
        new(fileName: Constants.FileNames.Dockerfile, isToGenerate: false),
        new(fileName: Constants.FileNames.DockerfileOnline, isToGenerate: false),
        new(fileName: Constants.FileNames.DeploySheet, isToGenerate: false) // Format for getting the project name from the cli options
    };

    /// <summary>
    /// Interacts with the user via command line input to get the files to generate.
    /// </summary>    
    public List<CliFileGenerateModel> GetRequestedFilesToGenerate()
    {
        if (_cliOptions.Value.GenerateAllFiles)
        {
            var filesToGenerate = new List<CliFileGenerateModel>()
            {
                 Constants.FileNames.AppSettingsOnline,
                 Constants.FileNames.AppSettingsDocker
            };

            if (!string.IsNullOrWhiteSpace(_cliOptions.Value.DeployName) && !string.IsNullOrWhiteSpace(_cliOptions.Value.ImageName))
            {
                filesToGenerate.AddRange(new List<CliFileGenerateModel>()
                {
                     Constants.FileNames.K8sYaml,
                     Constants.FileNames.IsaBkoYaml,
                     Constants.FileNames.DeploySheet,
                     Constants.FileNames.Dockerfile,
                     Constants.FileNames.DockerfileOnline
                });
            }

            return filesToGenerate;
        }

        while (true)
        {
            Console.WriteLine(Constants.Messages.FILES_TO_GENERATE_INFO_MESSAGE);

            PrintCurrentOptionsChoices();

            var input = Console.ReadLine();
            if (input is Constants.DEFAULT_QUIT_CMD_FILES_SELECTION_CHAR)
            {
                break;
            }

            if (int.TryParse(input, out int index) && index > 0 && index <= _files.Count)
            {
                _files[index - 1] = new(_files[index - 1].FileName, !_files[index - 1].IsToGenerate);
                EnsureK8sDeploysGeneratedTogether(index);
            }
            else
            {
                Console.WriteLine(Constants.Messages.INVALID_INPUT_ERROR_MESSAGE);
                Console.ReadKey();
            }

            Console.Clear();
        }

        return _files.Where(f => f.IsToGenerate).ToList();
    }

    /// <summary>
    /// To generate Azul K8s Yaml, we need the ImageName provided
    /// To generate IsaBko Yaml, we get the image name based on the ImageName provided
    /// Therefore, here we ensure IsaBkoYaml is only generated together with K8sYaml
    /// </summary>
    /// <param name="index"></param>
    private void EnsureK8sDeploysGeneratedTogether(int index)
    {
        if (_files[index - 1].FileName is Constants.FileNames.IsaBkoYaml)
        {
            var file = _files[_files.IndexOf(_files.Where(f => f.FileName is Constants.FileNames.K8sYaml).First())];

            if (!file.IsToGenerate && _files[index - 1].IsToGenerate)
            {
                file.SetIsToGenerate(_files[index - 1].IsToGenerate);
            }
        }

        if (_files[index - 1].FileName is Constants.FileNames.K8sYaml)
        {
            var file = _files[_files.IndexOf(_files.Where(f => f.FileName is Constants.FileNames.IsaBkoYaml).First())];

            if (file.IsToGenerate && !_files[index - 1].IsToGenerate)
            {
                file.SetIsToGenerate(_files[index - 1].IsToGenerate);
            }
        }
    }

    public string GetDeployName()
    {
        return GetInput(Constants.Messages.GET_DEPLOY_NAME_MESSAGE);
    }

    public string GetImageName()
    {
        return GetInput(Constants.Messages.GET_IMAGE_NAME_MESSAGE);
    }

    private static string GetInput(string message)
    {
        while (true)
        {
            Console.WriteLine(message);

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(Constants.Messages.INVALID_INPUT_ERROR_MESSAGE);
                Console.ReadKey();
                continue;
            }

            return input.Trim();
        }
    }

    private void PrintCurrentOptionsChoices()
    {
        Console.Out.Write('\n');

        for (int i = 0; i < _files.Count; i++)
        {
            var fileChoice = _files[i];

            Console.WriteLine($"{i + 1} - [{(fileChoice.IsToGenerate ? 'x' : ' ')}] {string.Format(fileChoice.FileName, _cliOptions.Value.ApplicationName)}");
        }
    }
}
