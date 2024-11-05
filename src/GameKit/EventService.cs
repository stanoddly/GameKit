using System.Collections.Concurrent;
using GameKit.Input;
using SDL;

namespace GameKit;

public interface IEventProcessor
{
    public void Process();
}

public class EventService: IEventProcessor
{
    private readonly ConcurrentQueue<SDL_Event> _eventQueue = new();
    private readonly InputService _inputService;
    private readonly AppControl _appControl;

    public EventService(InputService inputService, AppControl appControl)
    {
        _inputService = inputService;
        _appControl = appControl;
    }

    public void Process()
    {
        while (_eventQueue.TryDequeue(out SDL_Event evt))
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

    internal unsafe void EnqueueEvent(SDL_Event* evt)
    {
        _eventQueue.Enqueue(*evt);
    }
}
