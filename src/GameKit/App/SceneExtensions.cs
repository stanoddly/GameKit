namespace GameKit.App;

public static class SceneExtensions
{
    public static GameKitAppBuilder UseScenes(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<SceneManager>();
        return builder;
    }
}
