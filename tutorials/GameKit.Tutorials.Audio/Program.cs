using System.Numerics;
using GameKit.App;
using GameKit.Audio;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Audio;

static class Program
{
    private const string BeepPath = "audio/beep-example.ogg";
    private const int SourceCount = 4;

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager()
            .RegisterAudio();

        builder.AddSingleton(new AppConfig { Size = (640, 480), Title = "Audio Tutorial" });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>, NullRenderPhase<DefaultRenderContext>>();

        builder.OnStart((IAudioSystem audioSystem, IKeyboardService keyboardService, AppControl appControl) =>
        {
            AudioBuffer beep = audioSystem.LoadBuffer(BeepPath);
            AudioSource[] sources = CreateSources(audioSystem, beep);
            AudioGroup currentGroup = audioSystem.Groups.Effects;
            float sourceX = 0.0f;
            int sourceIndex = 0;

            Console.WriteLine("Audio tutorial");
            Console.WriteLine("Space: play the beep");
            Console.WriteLine("Left/Right: move the source");
            Console.WriteLine("1: effects group, 2: UI group, 3: muted UI group");
            Console.WriteLine("Escape: quit");
            Console.WriteLine($"Source group: {currentGroup.Name}");
            Console.WriteLine($"Source position: {sourceX:0.0}");

            keyboardService.KeyDown += (Keyboard _, KeyEventArgs eventArgs) =>
            {
                if (eventArgs.Key == VirtualKey.Space)
                {
                    AudioSource source = sources[sourceIndex];
                    source.Group = currentGroup;
                    source.Position = new Vector3(sourceX, 0.0f, 0.0f);
                    source.Play();
                    sourceIndex = (sourceIndex + 1) % sources.Length;
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Left)
                {
                    sourceX -= 2.0f;
                    Console.WriteLine($"Source position: {sourceX:0.0}");
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Right)
                {
                    sourceX += 2.0f;
                    Console.WriteLine($"Source position: {sourceX:0.0}");
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Number1)
                {
                    currentGroup = audioSystem.Groups.Effects;
                    audioSystem.Groups.Effects.Gain = 1.0f;
                    Console.WriteLine($"Source group: {currentGroup.Name}");
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Number2)
                {
                    currentGroup = audioSystem.Groups.Ui;
                    audioSystem.Groups.Ui.Gain = 1.0f;
                    Console.WriteLine($"Source group: {currentGroup.Name}");
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Number3)
                {
                    currentGroup = audioSystem.Groups.Ui;
                    audioSystem.Groups.Ui.Gain = 0.0f;
                    Console.WriteLine($"Source group: {currentGroup.Name} muted");
                    eventArgs.Consume();
                    return;
                }

                if (eventArgs.Key == VirtualKey.Escape)
                {
                    appControl.Quit();
                    eventArgs.Consume();
                }
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }

    private static AudioSource[] CreateSources(IAudioSystem audioSystem, AudioBuffer buffer)
    {
        AudioSource[] sources = new AudioSource[SourceCount];
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = audioSystem.CreateSource();
            source.Buffer = buffer;
            source.Gain = 0.45f;
            source.Group = audioSystem.Groups.Effects;
            sources[i] = source;
        }

        return sources;
    }
}
