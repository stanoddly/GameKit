using GameKit;
using GameKit.Input;

using var gameKitApp = new GameKitAppBuilder()
    .AddContentFromProjectDirectory("Content")
    .Build();

gameKitApp.Input.KeyDown += (Keyboard sender, in KeyEventArgs eventArgs) =>
{
    Console.WriteLine("KeyDown!");
};

gameKitApp.Input.KeyUp += (Keyboard sender, in KeyEventArgs eventArgs) =>
{
    Console.WriteLine("KeyUp!");
};

return gameKitApp.Run();