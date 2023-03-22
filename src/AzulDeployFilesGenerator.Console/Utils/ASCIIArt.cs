namespace AzulDeployFilesGenerator.Console.Utils;

internal static class ASCIIArt
{
    internal static void PrintWelcome()
    {
        System.Console.WriteLine(
            FiggleFonts.Standard.Render("AZUL"));

        System.Console.WriteLine(
            FiggleFonts.Standard.Render("D F G"));

        System.Console.WriteLine("\n");
    }
}
