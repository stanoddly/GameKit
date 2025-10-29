using System.Diagnostics;
using System.Text.Json;
using GameKit.Shaders;

namespace GameKit.SlangShaderPack;

public static class SlangCompiler
{
    private const string SlangCompilerExecutablePath = "/opt/slang/bin/slangc";

    private static readonly Dictionary<ShaderFormat, string> TargetsWithExtensions = new()
    {
        [ShaderFormat.SpirV] = "spv",
        //["dxbc"] = "dxbc",
        [ShaderFormat.Dxil] = "dxil",
        [ShaderFormat.Msl] = "metal",
        //["metallib"] = "metallib"
    };

    private static string GetTarget(ShaderFormat format)
    {
        return format switch
        {
            ShaderFormat.SpirV => "spirv",
            ShaderFormat.Dxil => "dxil",
            ShaderFormat.Msl => "metal",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    // /opt/slang/bin/slangc PositionColor.slang -profile glsl_450 -target spirv -o PositionColor.spv
    public static void CompileIt(FileInfo fileInfo, int minimalYear)
    {
        DirectoryInfo? parentDirectoryInfo = Directory.GetParent(fileInfo.FullName);
        string filename = fileInfo.FullName;
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        if (parentDirectoryInfo == null)
        {
            throw new Exception();
        }
            
        Console.WriteLine($"Processing file: {filename}");
        Console.WriteLine($"Result directory: {parentDirectoryInfo.FullName}");
        
        // TODO: do we want to delete it?
        DirectoryInfo temporaryDirectory = Directory.CreateTempSubdirectory("ShaderPack_");;
        
        Console.WriteLine($"Intermediate results written to: {temporaryDirectory.FullName}");

        using Process p = new Process();

        ProcessStartInfo info = new ProcessStartInfo(SlangCompilerExecutablePath);
        string reflectionFilename = Path.Join(temporaryDirectory.FullName, "reflection.json");
        string args = $"\"{filename}\" -warnings-disable 39013,39001 -reflection-json {reflectionFilename}";

        List<(ShaderFormat, string)> targetAndFilename = new();
        foreach ((ShaderFormat format, string extension) in TargetsWithExtensions)
        {
            string target = GetTarget(format);
            string outputFile = Path.Join(temporaryDirectory.FullName, $"{filenameWithoutExtension}.{extension}");

            if (ShaderProfiles.TryGetProfileForTarget(target, minimalYear, out string? profile))
            {
                args += $" -target \"{target}\" -profile \"{profile}\" -o \"{outputFile}\"";
            }
            else
            {
                args += $" -target \"{target}\" -o \"{outputFile}\"";
            }

            if (format == ShaderFormat.SpirV)
            {
                args += " -emit-spirv-via-glsl";
            }
            
            targetAndFilename.Add((format, outputFile));
        }

        info.Arguments = args;
        info.UseShellExecute = false;
        p.StartInfo = info;
        
        Console.WriteLine($"Executing: {SlangCompilerExecutablePath} {args}");

        p.Start();

        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new NotImplementedException();
        }
        
        SlangReflectionInfo slangReflectionInfo = SlangReflectionInfoLoader.Load(reflectionFilename);

        List<ShaderInstance> shaderInstance = new();
        foreach ((ShaderFormat target, string targetResultFilename) in targetAndFilename)
        {
            string relativeFilename = Path.GetFileName(targetResultFilename);
            string copyFilename = Path.Combine(parentDirectoryInfo.FullName, relativeFilename);
            shaderInstance.Add(new ShaderInstance{Filename = relativeFilename, Format = target, EntryPoint = slangReflectionInfo.EntryPointName});
            
            File.Copy(targetResultFilename, copyFilename, true);
        }

        ShaderResources shaderResources = slangReflectionInfo.Resources;

        ShaderMetadata shaderMetadata = new()
        {
            Stage = slangReflectionInfo.Stage,
            Resources = shaderResources,
            Shaders = shaderInstance
        };
        Console.WriteLine($"Stage: {slangReflectionInfo.Stage.ToString()}");

        string shaderMetadataFilename = Path.Combine(parentDirectoryInfo.FullName, $"{filenameWithoutExtension}.metadata.json");
        using FileStream fileStream = File.Create(shaderMetadataFilename);

        JsonSerializer.Serialize(fileStream, shaderMetadata, ShaderMetadataJsonContext.Default.ShaderMetadata);
    }
}