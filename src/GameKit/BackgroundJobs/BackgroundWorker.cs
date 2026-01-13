using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundWorker
{
    private readonly BackgroundWorkHub _hub;
    private readonly List<HandlerRegistration> _registrations;
    private readonly CancellationToken _cancellationToken;
    private readonly IGpuDevice _gpuDevice;

    private List<BackgroundHandlerWrapper?> _handlers = null!;

    public BackgroundWorker(
        BackgroundWorkHub hub,
        List<HandlerRegistration> registrations,
        CancellationToken cancellationToken,
        IGpuDevice gpuDevice)
    {
        _hub = hub;
        _registrations = registrations;
        _cancellationToken = cancellationToken;
        _gpuDevice = gpuDevice;
    }

    public void Run()
    {
        _handlers = CreateHandlerLookup();

        while (!_cancellationToken.IsCancellationRequested)
        {
            if (_hub.TryDequeueBackgroundMessage(out BackgroundMessage message))
            {
                HandleMessage(message);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private List<BackgroundHandlerWrapper?> CreateHandlerLookup()
    {
        List<BackgroundHandlerWrapper?> handlers = [null];

        foreach (HandlerRegistration registration in _registrations)
        {
            while (handlers.Count <= registration.MessageTypeId)
            {
                handlers.Add(null);
            }

            handlers[registration.MessageTypeId] = registration.CreateWrapper();
        }

        return handlers;
    }

    private void HandleMessage(BackgroundMessage message)
    {
        BackgroundHandlerWrapper? handler = message.TypeId < _handlers.Count ? _handlers[message.TypeId] : null;
        if (handler == null)
        {
            return;
        }

        using CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();

        using (ICopyPass copyPass = commandBuffer.CreateCopyPass())
        {
            BackgroundWorkContext context = new(copyPass);
            handler.Handle(message.Payload, context, _hub);
        }

        using GpuFence fence = commandBuffer.SubmitAndAcquireFence();
        _gpuDevice.WaitForFences([fence]);
    }
}
