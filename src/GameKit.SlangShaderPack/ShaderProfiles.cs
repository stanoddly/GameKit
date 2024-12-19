namespace GameKit.SlangShaderPack;

public static class ShaderProfiles
{
    public const int MinimalYear = 2015;
    public static readonly List<(int Year, string Profile)> DxbcTimeline = new()
    {
        (2009, "sm_4_0"),
        (2012, "sm_5_0"),
        (2015, "sm_5_1")
    };

    public static readonly List<(int Year, string Profile)> DxilTimeline = new()
    {
        (2015, "sm_6_0"),
        (2017, "sm_6_1"),
        (2020, "sm_6_5"),
        (2022, "sm_6_6")
    };

    public static readonly List<(int Year, string Profile)> SpirvTimeline = new()
    {
        (2015, "glsl_450"),
        (2017, "glsl_460")
    };

    public static bool TryGetProfileForTarget(string target, int year, out string? profile)
    {
        List<(int Year, string Profile)>? timeline = target switch
        {
            "dxbc" => DxbcTimeline,
            "dxil" => DxilTimeline,
            "spirv" => SpirvTimeline,
            _ => null
        };

        if (timeline == null)
        {
            profile = null;
            return false;
        }

        profile = timeline
                   .Where(x => x.Year <= year)
                   .MaxBy(x => x.Year)
                   .Profile ;
        return true;
    }
}