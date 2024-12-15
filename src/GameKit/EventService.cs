using GameKit.Input;
using SDL;

namespace GameKit;

public interface IEventProcessor
{
    public void Process();
}

public class EventService: IEventProcessor
{
    private readonly InputService _inputService;
    private readonly AppControl _appControl;

    internal EventService(InputService inputService, AppControl appControl)
    {
        _inputService = inputService;
        _appControl = appControl;
    }

    public void Process()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt) == true)
            {
                if (evt.Type == SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    _inputService.OnKeyEvent(evt.key);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    _inputService.OnKeyEvent(evt.key);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
                {
                    _appControl.Quit();
                }
            }
        }
    }
}
