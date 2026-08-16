# Window rendering

GameKit runs every active `IRenderCoordinator` once per frame. Coordinators registered by an active stage are discovered through the root coordinator registry and are removed when the stage provider is disposed.

Each render-context type identifies one rendering graph in a service-provider hierarchy. Its `IRenderer<TContext>` registrations can come from the root provider or the active stage. Registering a second coordinator for the same context type is rejected; use another context type for an independent graph.

Each window rendering graph is identified by its render-context type. Its `Window<TContext>` is a DI-owned singleton created when the owning service provider is built, so a secondary window cannot exist without a corresponding rendering registration.

## Primary window

`UseDefaultRendering()` creates `Window<DefaultRenderContext>` and registers its rendering graph. Omitting it leaves the application without a default window:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseDefaultRendering();

builder.AddSingleton<IRenderer<DefaultRenderContext>>(GameRenderer.Create);
```

`DefaultRenderContext.Window` provides the native window associated with the current swapchain texture.
Services can inject the primary window as `Window<DefaultRenderContext>`.

## Stage-owned secondary window

Register a separate context graph and its window configuration on the stage's existing `ServiceCollection`. Building the stage provider creates the secondary window without creating another service provider for it:

```csharp
stages.Load(services =>
{
    services.UseWindowRendering<InventoryRenderContext>(
        new WindowConfig(
            Size: new Size<uint>(480, 360),
            Title: "Inventory"),
        InventoryRenderContext.Create);
    services.AddSingleton<IRenderer<InventoryRenderContext>>(InventoryRenderer.Create);
    services.AddSingleton<GameController>();
});
```

The context factory receives the resolved window, its swapchain texture, and the command buffer:

```csharp
public static InventoryRenderContext Create(
    Window<InventoryRenderContext> window,
    SwapchainTexture swapchainTexture,
    CommandBuffer commandBuffer)
{
    return new InventoryRenderContext(window, swapchainTexture, commandBuffer);
}
```

Services owned by the same provider can inject the typed window directly:

```csharp
public sealed class InventoryController
{
    private readonly Window<InventoryRenderContext> _window;

    public InventoryController(Window<InventoryRenderContext> window)
    {
        _window = window;
    }
}
```

Disposing the stage unregisters and disposes its secondary window. The primary window and its rendering graph are unaffected.

## Composing rendering for one window

Use multiple `IRenderer<TContext>` registrations to compose rendering for one window. They execute in order with the same context, command buffer, and swapchain texture. A later render pass using `Clear` replaces existing contents; a pass using `Load` can draw over them.

The complete menu → game stage → secondary window flow is in `tutorials/GameKit.Tutorials.MultiWindow`.
