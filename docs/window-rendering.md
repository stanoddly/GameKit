# Window rendering

GameKit runs every active `IRenderCoordinator` once per frame. Coordinators registered by an active stage are discovered through the root coordinator registry and are removed when the stage provider is disposed.

Each render-context type identifies one rendering graph in a service-provider hierarchy. Its `IRenderer<TContext>` registrations can come from the root provider or the active stage. Registering a second coordinator for the same context type is rejected; use another context type for an independent graph.

## Primary window

`UseDefaultRendering()` registers the `DefaultRenderContext` graph and attaches it to the primary window:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseDefaultRendering();

builder.AddSingleton<IRenderer<DefaultRenderContext>>(GameRenderer.Create);
```

`DefaultRenderContext.Window` identifies the window associated with the current swapchain texture.

## Stage-owned secondary window

Register a separate context graph on the stage's existing `ServiceCollection`. This does not create a service provider for the window:

```csharp
stages.Load(services =>
{
    services.UseWindowRendering<SecondaryRenderContext>(SecondaryRenderContext.Create);
    services.AddSingleton<IRenderer<SecondaryRenderContext>>(SecondaryRenderer.Create);
    services.AddSingleton<GameController>();
});
```

The context factory receives the resolved window, its swapchain texture, and the command buffer:

```csharp
public static SecondaryRenderContext Create(
    Window window,
    SwapchainTexture swapchainTexture,
    CommandBuffer commandBuffer)
{
    return new SecondaryRenderContext(window, swapchainTexture, commandBuffer);
}
```

The coordinator starts unattached and therefore does no work. Runtime code creates a window and attaches the graph using an opaque `WindowId`:

```csharp
WindowId windowId = windows.CreateWindow(new WindowOptions(Title: "Inventory"));
IWindowRenderBinding binding = secondaryRendering.Attach(windowId);
```

Only one window can be attached to a context graph at a time. Disposing the binding, or disposing the stage-owned coordinator, detaches the graph and destroys its secondary window. If the user closes the window first, the binding becomes inactive and subsequent coordinator executions are no-ops. The primary window and its rendering graph are unaffected by the stage lifecycle.

The complete menu → game stage → secondary window flow is in `tutorials/GameKit.Tutorials.MultiWindow`.
