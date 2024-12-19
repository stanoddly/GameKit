using System.Diagnostics;

namespace GameKit.SlangShaderPack;

public static class SlangCompiler
{
    private static readonly Dictionary<string, string> TargetsWithExtensions = new()
    {
        ["spirv"] = "spv",
        //["dxbc"] = "dxbc",
        //["dxil"] = "dxil",
        //["metal"] = "metal",
        //["metallib"] = "metallib"
    };
    
    // /opt/slang/bin/slangc PositionColor.slang -profile glsl_450 -target spirv -o PositionColor.spv
    public static void CompileIt(string path, int minimalYear)
    {
        Console.WriteLine($"Processing file: {path}");
        DirectoryInfo? dir = null;
        
        // TODO: do we want to delete it?
        dir = Directory.CreateTempSubdirectory("ShaderPack_");
        
        Console.WriteLine($"Intermediate results written to: {dir.FullName}");

        using Process p = new Process();

        ProcessStartInfo info = new ProcessStartInfo("/opt/slang/bin/slangc");
        string reflectionFilename = Path.Join(dir.FullName, "reflection.json");
        string args = $"\"{path}\" -reflection-json {reflectionFilename}";

        foreach ((string target, string extension) in TargetsWithExtensions)
        {
            string outputFile = Path.Join(dir.FullName, $"{target}.{extension}");

            if (ShaderProfiles.TryGetProfileForTarget(target, minimalYear, out string? profile))
            {
                args += $" -target \"{target}\" -profile \"{profile}\" -o \"{outputFile}\"";
            }
            else
            {
                args += $" -target \"{target}\" -o \"{outputFile}\"";
            }
        }

        info.Arguments = args;
        info.UseShellExecute = false;
        p.StartInfo = info;

        p.Start();

        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new NotImplementedException();
        }
    }
    
    
}