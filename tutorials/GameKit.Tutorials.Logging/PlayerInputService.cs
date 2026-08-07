using GameKit.App;
using GameKit.Input;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameKit.Tutorials.Logging;

sealed class PlayerInputService : IDisposable
{
    private readonly IKeyboardService _keyboardService;
    private readonly AppControl _appControl;
    private readonly ILogger _logger;

    public static PlayerInputService Create(
        IKeyboardService keyboardService,
        AppControl appControl,
        ILogger logger)
    {
        PlayerInputService service = new PlayerInputService(keyboardService, appControl, logger);
        logger.ZLogInformation($"Player input service started; press Escape to quit");
        return service;
    }

    private PlayerInputService(
        IKeyboardService keyboardService,
        AppControl appControl,
        ILogger logger)
    {
        _keyboardService = keyboardService;
        _appControl = appControl;
        _logger = logger;

        _keyboardService.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(Keyboard _, KeyEventArgs eventArgs)
    {
        _logger.ZLogConditionalDebug($"Key {eventArgs.Key} pressed using scancode {eventArgs.Scancode}");

        if (eventArgs.Key == VirtualKey.Escape)
        {
            _logger.ZLogInformation($"Quit requested from keyboard input");
            _appControl.Quit();
            eventArgs.Consume();
        }
    }

    public void Dispose()
    {
        _keyboardService.KeyDown -= OnKeyDown;
    }
}
