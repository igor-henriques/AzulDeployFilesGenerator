using Console = System.Console;

namespace AzulDeployFileGenerator.Infrastructure.CLI;

public sealed class CliService : ICliService
{
    private readonly List<CliFileGenerateModel> _files = new()
    {
        new("appsettings.Docker.json", false),
        new("appsettings.Online.json", false)
        //new("k8sdeploy.yaml", true),
        //new("tokenizer sheet", true)
    };

    /// <summary>
    /// Interacts with the user via command line input to get the files to generate.
    /// </summary>
    /// <returns></returns>
    public List<CliFileGenerateModel> GetRequestedFilesToGenerate()
    {
        while (true)
        {
            Console.Out.WriteLine("\nWrite the relative number to set as false/true, or 'q' to leave:");

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
                Console.WriteLine("Invalid input. Press any key to continue...");
                Console.ReadKey();
            }

            Console.Clear();
        }

        return _files.Where(f => f.IsToGenerate).ToList();
    }

    private void PrintCurrentOptionsChoices()
    {
        Console.Out.Write("\n");

        for (int i = 0; i < _files.Count; i++)
        {
            var fileChoice = _files[i];
            Console.Out.WriteLine($"{i + 1} - [{(fileChoice.IsToGenerate ? 'x' : ' ')}] {fileChoice.FileName}");
        }
    }
}
