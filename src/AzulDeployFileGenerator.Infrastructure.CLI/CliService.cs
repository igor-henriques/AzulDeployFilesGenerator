using Microsoft.Extensions.Options;
using Console = System.Console;

namespace AzulDeployFileGenerator.Infrastructure.CLI;

public sealed class CliService : ICliService
{
    private readonly IOptions<CliCommandOptions> _cliOptions;    

    public CliService(IOptions<CliCommandOptions> cliOptions)
    {
        _cliOptions = cliOptions;
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
        while (true)
        {
            Console.Out.WriteLine(Constants.Messages.FILES_TO_GENERATE_INFO_MESSAGE);

            PrintCurrentOptionsChoices();

            var input = Console.ReadLine();
            if (input is "q")
            {
                break;
            }

            if (int.TryParse(input, out int index) && index > 0 && index <= _files.Count)
            {
                _files[index - 1] = new(_files[index - 1].FileName, !_files[index - 1].IsToGenerate);
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

    public string GetDeployName()
    {        
        while (true)
        {
            Console.Out.WriteLine(Constants.Messages.GET_DEPLOY_NAME_MESSAGE);

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Out.WriteLine(Constants.Messages.INVALID_INPUT_ERROR_MESSAGE);
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
            Console.Out.WriteLine($"{i + 1} - [{(fileChoice.IsToGenerate ? 'x' : ' ')}] {string.Format(fileChoice.FileName, _cliOptions.Value.ApplicationName)}");
        }
    }
}
