# Window rendering

GameKit uses `default(ViewScope)` for the ordinary single-window case. Applications only need to
name scopes when they render more than one window.

## Single-window rendering

`UseDefaultRendering` creates a DI-owned window and render coordinator:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseDefaultRendering(
        new WindowConfig(
            Size: new Size<uint>(1280, 720),
            Title: "Game"));
```

Omitting `UseDefaultRendering` creates no window.

Window renderers use the ordinary `IRenderer<DefaultRenderContext>` contract:

```csharp
public sealed class GameRenderer : IRenderer<DefaultRenderContext>
{
    public void Render(DefaultRenderContext renderContext)
    {
        // Record rendering commands.
    }
}
```

Register renderers normally through DI:

```csharp
builder.AddSingleton<IRenderer<DefaultRenderContext>, GameRenderer>(GameRenderer.Create);
```

The default `IViewScoped.ViewScope` implementation returns `default`, so single-window renderers do
not declare a scope.

The default window is available without a scope argument:

```csharp
Window window = windowRegistry.GetWindow();
graphicsPipelineBuilder.AddColorFormatFromDisplay();
textInputService.Start();
bool containsMouse = mouseService.IsInWindow();
```

## Multiple windows

Define stable non-negative scope values for additional windows:

```csharp
internal static class ViewScopes
{
    internal static readonly ViewScope Inventory = new(1);
}
```

The implicit window remains `default(ViewScope)` while additional windows receive explicit scopes:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseDefaultRendering(
        new WindowConfig(
            Size: new Size<uint>(1280, 720),
            Title: "Game"))
    .UseDefaultRendering(
        ViewScopes.Inventory,
        new WindowConfig(
            Size: new Size<uint>(480, 360),
            Title: "Inventory"));
```

A renderer for an additional window overrides the scope explicitly:

```csharp
public sealed class InventoryRenderer : IRenderer<DefaultRenderContext>
{
    ViewScope IViewScoped.ViewScope => ViewScopes.Inventory;

    public void Render(DefaultRenderContext renderContext)
    {
        // Render the inventory window.
    }
}
```

The renderer registry preserves `IOrderable.Order` and executes each renderer only for its matching
scope. A reusable renderer can receive its `ViewScope` through construction and be registered more
than once.

Resolve resources for additional windows through their scope:

```csharp
Window inventoryWindow = windowRegistry.GetWindow(ViewScopes.Inventory);
graphicsPipelineBuilder.AddColorFormatFromDisplay(ViewScopes.Inventory);
```

SDL window IDs remain internal and are used only to route native events. Windows registered by a
stage use the stage provider's lifetime. Disposing that provider unregisters and disposes its window
and render coordinator.

## Scoped input

Window-associated events and subscriptions target the implicit default scope. In a multi-window
application, use a scoped subscription when a handler belongs to another window:

```csharp
keyboardService.SubscribeKeyDown(
    ViewScopes.Inventory,
    priority: 0,
    (keyboard, eventArgs) => HandleInventoryKey(keyboard, eventArgs));
```

Scoped overloads exist for keyboard, mouse, and text-input subscriptions.

## Pencuil

The common case requires no scope:

```csharp
builder.UsePencuil();
```

Configure another Pencuil instance only for an additional window:

```csharp
builder.UsePencuil(ViewScopes.Inventory);
```

Pencuil's MVVM contracts use explicit names: `IPencuilView`, `IPencuilViewModel`, and
`PencuilView<TViewModel>`. Their default scope is implicit; views belonging to another window
override `IViewScoped.ViewScope` or pass a scope to the Pencuil view base class.

See `GameKit.Tutorials.MultiWindow` for two independently rendered windows and
`GameKit.Tutorials.MultiWindowTextInput` for independent Pencuil focus and text input.
