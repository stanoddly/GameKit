using GameKit;
using GameKit.Input;using GameKit.Modules;

GameKitApp gameKitApp = new GameKitApp()
    .AddScoped<MyInputHandler>();

return gameKitApp.Run();


public class MyInputHandler(InputService inputService) : IPreparable
{
    public void Prepare()
    {
        inputService.KeyDown += (Keyboard keyboard, in KeyEventArgs eventArgs) =>
        {
            Console.WriteLine($"Key down! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
        };

        inputService.KeyUp += (Keyboard keyboard, in KeyEventArgs eventArgs) =>
        {
            Console.WriteLine($"Key up! Scancode {eventArgs.Scancode}, key {eventArgs.Key}!");
        };
    }
}
