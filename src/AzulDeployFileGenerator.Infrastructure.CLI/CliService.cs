using Console = System.Console;

namespace AzulDeployFileGenerator.Infrastructure.CLI;

public sealed class CliService : ICliService
{
    private readonly List<CliFileGenerateModel> _files = new()
    {
        new(fileName: "appsettings.Docker.json", isToGenerate: false),
        new(fileName: "appsettings.Online.json", isToGenerate:false),
        new(fileName: "k8sdeploy.yaml", isToGenerate:false),
        new(fileName: "tokenizer sheet", isToGenerate:false)
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

    private void PrintCurrentOptionsChoices()
    {
        Console.Out.Write('\n');

        for (int i = 0; i < _files.Count; i++)
        {
            var fileChoice = _files[i];
            Console.Out.WriteLine($"{i + 1} - [{(fileChoice.IsToGenerate ? 'x' : ' ')}] {fileChoice.FileName}");
        }
    }
}
