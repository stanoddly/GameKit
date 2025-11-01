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

        var onlySpirvOption = new Option<bool>(
            name: "--only-spirv"
        )
        {
            Description = "Compile only SPIR-V target, skip other targets"
        };

        var forceOption = new Option<bool>(
            name: "--force")
        {
            Description = "Force compilation even if source file hash hasn't changed"
        };

        var rootCommand = new RootCommand("Compile Slang shaders to various targets")
        {
            filenamesOption,
            onlySpirvOption,
            forceOption
        };

        var parseResult = rootCommand.Parse(args);

        SdlangCompiler sdlangCompiler = new();
        sdlangCompiler.Compile(parseResult.GetValue(filenamesOption),parseResult.GetValue(onlySpirvOption), parseResult.GetValue(forceOption));

        return 0;
    }
}