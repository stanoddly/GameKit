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

    /// <summary>
    /// Path to the slangc executable. Slang is build-time tooling, so it is located through this
    /// parameter instead of being copied into the output of the project being built.
    /// Deliberately not [Required], so that a project still calling the task the old way gets the
    /// migration hint below instead of MSBuild's generic missing-parameter error.
    /// </summary>
    public string? SlangCompilerPath { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(InputFile))
        {
            return true;
        }

        if (string.IsNullOrEmpty(SlangCompilerPath))
        {
            Log.LogError(
                "SdlangCompileTask requires SlangCompilerPath. Projects normally no longer invoke this task " +
                "directly: delete your CompileSdlangShaders target and declare the shaders instead, for example " +
                "<ItemGroup><SdlangShader Include=\"Content\\shaders\\*.slang\" /></ItemGroup>. " +
                "If you need a custom target, pass SlangCompilerPath=\"$(SlangCompilerPath)\". See docs/shaders.md.");
            return false;
        }

        try
        {
            SdlangCompiler sdlangCompiler = new(SlangCompilerPath);
            sdlangCompiler.Compile([InputFile], false);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError(ex.Message);
            return false;
        }
    }
}
