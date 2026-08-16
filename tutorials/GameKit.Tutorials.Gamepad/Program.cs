using System.Numerics;
using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Gamepad;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering(new WindowConfig { Size = (640, 480), Title = "Gamepad Tutorial" });

        builder.AddSingleton<IRenderer<DefaultRenderContext>, NullRenderer<DefaultRenderContext>>();

        builder.OnStart((IGamepadService gamepadService) =>
        {
            Console.WriteLine($"Gamepads connected at startup: {gamepadService.Gamepads.Count}");
            foreach (GameKit.Input.Gamepad gamepad in gamepadService.Gamepads)
            {
                Console.WriteLine($"  Gamepad {gamepad.DeviceId}");
            }

            if (gamepadService.Gamepads.Count == 0)
            {
                Console.WriteLine("No gamepads detected. Connect a gamepad and it will be picked up automatically.");
            }

            Console.WriteLine("Listening for gamepad input...");

            gamepadService.LeftStickMotion += (gamepad, motion) =>
            {
                Vector2 value = motion.Value;
                Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Left Stick: ({value.X:F2}, {value.Y:F2})");
            };

            gamepadService.RightStickMotion += (gamepad, motion) =>
            {
                Vector2 value = motion.Value;
                Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Right Stick: ({value.X:F2}, {value.Y:F2})");
            };

            gamepadService.LeftTriggerMotion += (gamepad, motion) =>
            {
                Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Left Trigger: {motion.Value:F2}");
            };

            gamepadService.RightTriggerMotion += (gamepad, motion) =>
            {
                Console.WriteLine($"[Gamepad {gamepad.DeviceId}] Right Trigger: {motion.Value:F2}");
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
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
