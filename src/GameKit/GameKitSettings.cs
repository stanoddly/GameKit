namespace GameKit;

public static class GameKitSettings
{
    #if DEBUG
    public const bool IsDebugBuild = true;
    #else
    public const bool IsDebugBuild = false;
    #endif
}
