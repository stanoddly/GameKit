using GameKit;
using GameKit.App;
using GameKit.Input;
using GameKit.Modules;

namespace Samples.Keyboard;

public class MyGame(InputService inputService) : IInitializable
{
    public void Initialize()
    {
        inputService.KeyDown += (GameKit.Input.Keyboard keyboard, in KeyEventArgs eventArgs) =>
        {
            Console.WriteLine($"Key down! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
            Console.Out.Flush();
        };

        inputService.KeyUp += (GameKit.Input.Keyboard keyboard, in KeyEventArgs eventArgs) =>
        {
            Console.WriteLine($"Key up! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
            Console.Out.Flush();
        };
    }

    public static int Main()
    {
        GameKitApp gameKitApp = new GameKitApp()
            .AddScoped<MyGame>();

        return gameKitApp.Run();
    }
}