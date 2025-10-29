using GameKit.Componentize;

namespace GameKit.Uiui;

public abstract class GuiComponent: GameComponent
{
    private Widget? _widget;
    protected abstract Widget Build();

    protected override void OnAttach()
    {
        WidgetService widgetService = ServiceLocator.GetService<WidgetService>();

        _widget = Build();
        
        widgetService.AddWidget(_widget);
    }

    protected override void OnDetach()
    {
        if (_widget != null)
        {
            WidgetService widgetService = ServiceLocator.GetService<WidgetService>();
            widgetService.RemoveWidget(_widget);
        }
    }
}
