using System.CommandLine;

namespace GameKit.SlangShaderPack;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArgument = new Argument<FileInfo>(
            name: "filename",
            description: "The file to process");

        var yearOption = new Option<int>(
            name: "--year",
            description: "Minimal year for shader support of GPUs",
            getDefaultValue: () => Math.Max(DateTime.Now.AddYears(-10).Year, ShaderProfiles.MinimalYear));

        // var commandCommand = new Command("compile", "Process the specified file")
        var rootCommand = new RootCommand("Process the specified file")
        {
            fileArgument,
            yearOption
        };

        rootCommand.SetHandler(ReadFile, fileArgument, yearOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void ReadFile(FileInfo file, int minimalYear)
    {
        SlangCompiler.CompileIt(file.FullName, minimalYear);
    }
}
