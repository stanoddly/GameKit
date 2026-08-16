# Window rendering

GameKit groups each window and its View-side resources through an application-defined `ViewScope`.
A scope associates a window, renderers, routed input, and optional Pencuil state without using a
render-context type as an identity.

Define stable scope values in the application:

```csharp
internal static class ViewScopes
{
    internal static readonly ViewScope Main = new(0);
    internal static readonly ViewScope Inventory = new(1);
}
```

Scope values must be non-negative. GameKit assigns no primary or default meaning to any value.

## Registering windows and renderers

`UseWindowRendering` creates one DI-owned window and render coordinator for a scope:

```csharp
GameKitAppBuilder builder = new GameKitAppBuilder()
    .UseWindowRendering(
        ViewScopes.Main,
        new WindowConfig(
            Size: new Size<uint>(1280, 720),
            Title: "Game"))
    .UseWindowRendering(
        ViewScopes.Inventory,
        new WindowConfig(
            Size: new Size<uint>(480, 360),
            Title: "Inventory"));
```

Omitting `UseWindowRendering` creates no window.

Renderers implement `IViewRenderer` and declare the scope to which the instance belongs:

```csharp
public sealed class InventoryRenderer : IViewRenderer
{
    public ViewScope ViewScope => ViewScopes.Inventory;

    public void Render(ViewRenderContext renderContext)
    {
        // Record rendering commands.
    }
}
```

Register renderers normally through DI:

```csharp
builder.AddSingleton<IViewRenderer, InventoryRenderer>(InventoryRenderer.Create);
```

The renderer registry preserves `IOrderable.Order` and executes a renderer only for the matching
scope. A reusable renderer can receive its `ViewScope` through its constructor and be registered more
than once.

## Resolving a window

Multiple windows cannot be distinguished through unkeyed `Window` injection. Resolve a specific
window through `WindowRegistry`:

```csharp
Window inventoryWindow = windowRegistry.GetWindow(ViewScopes.Inventory);
```

The registry exposes logical `ViewScope` lookup. SDL window IDs remain internal and are used only to
route native events.

Windows registered by a stage use the stage provider's lifetime. Disposing that provider unregisters
and disposes its window and render coordinator.

## Scoped input

Window-associated event arguments expose their source `ViewScope`. Prefer scoped subscriptions when
a handler belongs to one View:

```csharp
keyboardService.SubscribeKeyDown(
    ViewScopes.Inventory,
    priority: 0,
    (keyboard, eventArgs) => HandleInventoryKey(keyboard, eventArgs));
```

Scoped overloads exist for keyboard, mouse, and text-input subscriptions. Global events and
subscriptions remain available for application-wide shortcuts and diagnostics.

Text-input activation and window-focused mouse queries also use the scope:

```csharp
textInputService.Start(ViewScopes.Inventory);
bool containsMouse = mouseService.IsInWindow(ViewScopes.Inventory);
```

## Pencuil

Configure Pencuil independently for each scope:

```csharp
builder
    .UsePencuil(ViewScopes.Main)
    .UsePencuil(ViewScopes.Inventory);
```

Pencuil's MVVM contracts use explicit names: `IPencuilView`, `IPencuilViewModel`, and
`PencuilView<TViewModel>`. Each `IPencuilView` implements `IViewScoped`, allowing its instance to join
the matching Pencuil state.

See `GameKit.Tutorials.MultiWindow` for two independently rendered windows and
`GameKit.Tutorials.MultiWindowTextInput` for independent Pencuil focus and text input.
