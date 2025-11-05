using GameKit.SdlangCompileLib;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace GameKit.SdlangCompileTask;

/// <summary>
/// MSBuild task for compiling Slang shaders for SDL3.
/// </summary>
public class SdlangCompileTask : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// The input shader file to compile.
    /// </summary>
    [Required]
    public required string InputFile { get; set; }

    public override bool Execute()
    {
        SdlangCompiler sdlangCompiler = new();
        sdlangCompiler.Compile([InputFile], true, false);
        return true;
    }
}
