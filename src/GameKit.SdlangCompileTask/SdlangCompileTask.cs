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
    /// The input shader file to compile. If empty or null, the task succeeds without compiling.
    /// </summary>
    public string? InputFile { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(InputFile))
        {
            return true;
        }

        SdlangCompiler sdlangCompiler = new();
        sdlangCompiler.Compile([InputFile], false);
        return true;
    }
}
