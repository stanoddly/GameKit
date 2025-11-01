using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace GameKit.SdlangCompileTask;

/// <summary>
/// MSBuild task for compiling Slang shaders for SDL3.
/// </summary>
public class SdlangCompileTask : global::Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// The input shader file to compile.
    /// </summary>
    [Required]
    public string? InputFile { get; set; }

    /// <summary>
    /// Optional output file path. If not specified, output will be in the same directory as the input.
    /// </summary>
    public string? OutputFile { get; set; }

    public override bool Execute()
    {
        throw new NotImplementedException("SdlangCompileTask is not yet implemented.");
    }
}
