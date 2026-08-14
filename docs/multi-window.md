# Multi-window composition

Window identity and rendering contracts are separate typed DI boundaries:

- A concrete `Window` subclass identifies one logical window and owns its window-specific events and input services.
- `TRenderContext` identifies one render graph and selects `IRenderPhase<TRenderContext>` registrations.
- `WindowRenderContextProvider<TWindow, TRenderContext>` associates a window with a render graph and acquires its per-frame swapchain context.

Register additional windows and render graphs through the normal application service collection:

```csharp
public sealed class InspectorWindow : Window
{
}

builder.AddWindow<InspectorWindow>(new WindowOptions(
    Size: new Size<uint>(640, 480),
    Title: "Inspector"));

builder.UseDefaultRenderManager<InspectorRenderContext>();
builder.AddSingleton<
    IRenderContextProvider<InspectorRenderContext>,
    InspectorRenderContextProvider>();
builder.AddSingleton<
    IRenderPhase<InspectorRenderContext>,
    InspectorRenderer>();
```

Window subclasses are identity types. They must have a public parameterless constructor for factory creation and should be resolved from DI rather than constructed directly.

The application executes every registered render manager. Each manager sees only phases compatible with its render-context type. A provider derives from `WindowRenderContextProvider<TWindow, TRenderContext>` to bind context acquisition to its typed window.

SDL keyboard, mouse, text-input, and window-presence events are routed by native window ID. Consumers inject `IKeyboardService<TWindow>`, `IMouseService<TWindow>`, or `ITextInputService<TWindow>` to receive events for one window without filtering. Window events such as `ResolutionChanged` remain instance events on the injected window subclass.

The existing non-generic `Window`, `IKeyboardService`, `IMouseService`, and `ITextInputService` registrations resolve the built-in `DefaultWindow` services for single-window applications.
