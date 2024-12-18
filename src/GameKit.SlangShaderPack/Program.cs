using System.CommandLine;

namespace GameKit.ShaderPack;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArgument = new Argument<FileInfo>(
            name: "filename",
            description: "The file to process");

        var commandCommand = new Command("command", "Process the specified file")
        {
            fileArgument
        };

        commandCommand.SetHandler(async (FileInfo file) =>
        {
            Console.WriteLine($"Processing file: {file.FullName}");
        }, fileArgument);

        var rootCommand = new RootCommand("Sample file processing tool");
        rootCommand.AddCommand(commandCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static void ReadFile(FileInfo file)
    {
        File.ReadLines(file.FullName).ToList()
            .ForEach(line => Console.WriteLine(line));
    }
}