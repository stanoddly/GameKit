using GameKit;
using GameKit.Common;
using GameKit.Content;
using GameKit.Input;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.Gamepad;

[Module]
partial class GamepadApp : GameKitApp, IDefaultRenderContext
{
    public override AppConfig AppConfig { get; } = new() { Size = (640, 480), Title = "Gamepad Tutorial" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder().Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton]
    public partial NullRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);

    [OnActivate]
    void SetupGamepad(IGamepadService gamepadService)
    {
        Console.WriteLine($"Gamepads connected at startup: {gamepadService.Gamepads.Count}");
        foreach (Input.Gamepad gp in gamepadService.Gamepads)
        {
            Console.WriteLine($"  Gamepad {gp.DeviceId}");
        }

        if (gamepadService.Gamepads.Count == 0)
        {
            Console.WriteLine("No gamepads detected. Connect a gamepad and it will be picked up automatically.");
        }

        Console.WriteLine("Listening for gamepad input...");

        gamepadService.LeftStickMotion += (gamepad, motion) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Left Stick: ({motion.X:F2}, {motion.Y:F2})");
        };

        gamepadService.RightStickMotion += (gamepad, motion) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Right Stick: ({motion.X:F2}, {motion.Y:F2})");
        };

        gamepadService.LeftTriggerMotion += (gamepad, value) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Left Trigger: {value:F2}");
        };

        gamepadService.RightTriggerMotion += (gamepad, value) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Right Trigger: {value:F2}");
        };

        gamepadService.ButtonPress += (gamepad, button) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Button Pressed: {button}");
        };

        gamepadService.ButtonRelease += (gamepad, button) =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Button Released: {button}");
        };

        gamepadService.GamepadConnected += gamepad =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Connected");
        };

        gamepadService.GamepadDisconnected += gamepad =>
        {
            Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Disconnected");
        };
    }
}

static class Program
{
    static int Main()
    {
        using GamepadApp app = new();
        return app.Run();
    }
}
