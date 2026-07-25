using System.CommandLine;
using GameKit.SdlangCompileLib;

namespace GameKit.SdlangCompilerCli;

class Program
{
    static int Main(string[] args)
    {
        Argument<string[]> filenamesOption = new Argument<string[]>(
            name: "filenames")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "The shader file(s) or directory to process"
        };

        var forceOption = new Option<bool>(
            name: "--force")
        {
            Description = "Force compilation even if source file hash hasn't changed"
        };

        var rootCommand = new RootCommand("Compile Slang shaders to various targets")
        {
            filenamesOption,
            forceOption
        };

        var parseResult = rootCommand.Parse(args);

        try
        {
            SdlangCompiler sdlangCompiler = SdlangCompiler.CreateFromAssemblyDirectory();
            string[] filenames = parseResult.GetValue(filenamesOption) ?? [];
            sdlangCompiler.Compile(filenames, parseResult.GetValue(forceOption));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}