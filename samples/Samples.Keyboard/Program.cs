using GameKit;
using GameKit.Input;

using var gameKitApp = new GameKitAppBuilder()
    .AddContentFromProjectDirectory("Content")
    .Build();

gameKitApp.Input.KeyDown += (Keyboard keyboard, in KeyEventArgs eventArgs) =>
{
    Console.WriteLine($"Key down! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
};

gameKitApp.Input.KeyUp += (Keyboard keyboard, in KeyEventArgs eventArgs) =>
{
    Console.WriteLine($"Key up! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
};

return gameKitApp.Run();
