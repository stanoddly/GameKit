# Window rendering

GameKit runs every active `IRenderCoordinator` once per frame. Coordinators registered by an active stage are discovered through the root coordinator registry and are removed when the stage provider is disposed.

Each render-context type identifies one rendering graph in a service-provider hierarchy. Its `IRenderer<TContext>` registrations can come from the root provider or the active stage. Registering a second coordinator for the same context type is rejected; use another context type for an independent graph.

Each window also has a case-sensitive application-defined name. One coordinator claims each name, ensuring that a window has one swapchain acquisition and presentation per frame.

## Primary window

`UseDefaultRendering()` registers the `DefaultRenderContext` graph and claims the primary window named `"main"`:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseDefaultRendering();

builder.AddSingleton<IRenderer<DefaultRenderContext>>(GameRenderer.Create);
```

The name is also available as `WindowManager.PrimaryWindowName`. `DefaultRenderContext.Window` provides the native window associated with the current swapchain texture.

## Stage-owned secondary window

Register a separate context graph on the stage's existing `ServiceCollection`. This claims the window name without opening the native window or creating another service provider:

```csharp
stages.Load(services =>
{
    services.UseWindowRendering<InventoryRenderContext>(
        "inventory",
        InventoryRenderContext.Create);
    services.AddSingleton<IRenderer<InventoryRenderContext>>(InventoryRenderer.Create);
    services.AddSingleton<GameController>();
});
```

The context factory receives the resolved window, its swapchain texture, and the command buffer:

```csharp
public static InventoryRenderContext Create(
    Window window,
    SwapchainTexture swapchainTexture,
    CommandBuffer commandBuffer)
{
    return new InventoryRenderContext(window, swapchainTexture, commandBuffer);
}
```

The coordinator does no work while its claimed window is closed. Runtime code opens it by name:

```csharp
windows.CreateWindow(
    "inventory",
    new WindowOptions(Title: "Inventory"));
```

Creating an undeclared name, opening the same name twice, or registering another coordinator for a claimed name throws. A user-closed secondary window can be reopened under the same name while the stage remains active.

Disposing the stage releases its claim and closes the secondary window if it is open. The primary window and its rendering graph are unaffected.

## Composing rendering for one window

Different context types cannot claim the same window name. Multiple coordinators would acquire and present separate swapchain textures rather than compose reliably.

Use multiple `IRenderer<TContext>` registrations in one graph instead. They execute in order with the same context, command buffer, and swapchain texture. A later render pass using `Clear` replaces existing contents; a pass using `Load` can draw over them.

The complete menu → game stage → secondary window flow is in `tutorials/GameKit.Tutorials.MultiWindow`.
