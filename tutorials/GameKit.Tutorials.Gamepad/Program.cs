using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Gamepad;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRenderManager();

        builder.RegisterInstance(new AppConfig { Size = (640, 480), Title = "Gamepad Tutorial" });
        builder.RegisterType<NullRenderPhase<DefaultRenderContext>>().As<IRenderPhase<DefaultRenderContext>>();

        builder.OnStart((IGamepadService gamepadService) =>
        {
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
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
