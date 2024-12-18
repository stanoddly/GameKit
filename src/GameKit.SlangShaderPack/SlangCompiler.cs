using System.Diagnostics;
using System.Text;

namespace GameKit.ShaderPack;

public static class SlangCompiler
{
    private static Dictionary<string, string> _extensionLookup = new()
    {
        ["spirv"] = "spv",
        ["dxbc"] = "dxbc",
        ["dxil"] = "dxil",
        ["metal"] = "metal",
        ["metallib"] = "metallib"
    };
    
    // /opt/slang/bin/slangc PositionColor.slang -profile glsl_450 -target spirv -o PositionColor.spv
    public static void Compile(params string[] args)
    {
        using (Process p = new Process())
        {
            ProcessStartInfo info = new ProcessStartInfo("/opt/slang/bin/slangc");
            info.Arguments = string.Join(" ", args);
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            p.StartInfo = info;
            p.Start();

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new NotImplementedException();
            }
            
            string output = p.StandardOutput.ReadToEnd();
        }
    }
    
    // /opt/slang/bin/slangc PositionColor.slang -profile glsl_450 -target spirv -o PositionColor.spv
    public static void CompileIt(string path, (string, string)[] profilesAndTargets)
    {
        DirectoryInfo? dir = null;
        try
        {
            dir = Directory.CreateTempSubdirectory("ShaderPack_");

            using Process p = new Process();

            ProcessStartInfo info = new ProcessStartInfo("/opt/slang/bin/slangc");
            string reflectionFilename = Path.Join(dir.FullName, "reflection.json");
            string args = $"\"{path}\" -reflection-json {reflectionFilename}";

            foreach ((string target, string profile) in profilesAndTargets)
            {
                string extension = _extensionLookup[target];
                
                string outputFile = Path.Join(dir.FullName, $"{target}.{extension}");
                
                args += $" -target \"{target}\" -profile \"{profile}\" -o \"{outputFile}\" ";
            }

            info.Arguments = string.Join(" ", args);
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            p.StartInfo = info;

            p.Start();

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new NotImplementedException();
            }

            string output = p.StandardOutput.ReadToEnd();
        }
        finally
        {
            //dir?.Delete();
        }
    }
    
    
}