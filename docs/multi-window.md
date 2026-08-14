# Multi-window composition

Window identity and rendering contracts are separate typed DI boundaries:

- `Window<TWindow>` identifies one logical window and owns its window-specific events and input services.
- `TRenderContext` identifies one render graph and selects `IRenderPhase<TRenderContext>` registrations.
- `WindowRenderContextProvider<TWindow, TRenderContext>` associates a window with a render graph and acquires its per-frame swapchain context.

Register additional windows and render graphs through the normal application service collection:

```csharp
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

The application executes every registered render manager. Each manager sees only phases compatible with its render-context type. A provider derives from `WindowRenderContextProvider<TWindow, TRenderContext>` to bind context acquisition to its typed window.

SDL keyboard, mouse, text-input, and window-presence events are routed by native window ID. Consumers inject `IKeyboardService<TWindow>`, `IMouseService<TWindow>`, or `ITextInputService<TWindow>` to receive events for one window without filtering. Window events such as `ResolutionChanged` remain instance events on the injected `Window<TWindow>`.

The existing non-generic `Window`, `IKeyboardService`, `IMouseService`, and `ITextInputService` registrations resolve the built-in `DefaultWindow` services for single-window applications.
