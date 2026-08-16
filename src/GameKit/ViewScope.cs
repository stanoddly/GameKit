namespace GameKit;

public readonly record struct ViewScope(int Value);

public interface IViewScoped
{
    ViewScope ViewScope => default;
}
