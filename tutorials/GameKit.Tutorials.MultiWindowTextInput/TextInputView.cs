using GameKit.Gpu;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;
using GameKit.Text;

namespace GameKit.Tutorials.MultiWindowTextInput;

public sealed class TextInputViewModel<TRenderContext> : IViewModel
    where TRenderContext : IRenderContext
{
    private string _text = typeof(TRenderContext).Name;

    public bool IsDirty { get; set; } = true;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                IsDirty = true;
            }
        }
    }
}

public sealed class TextInputView<TRenderContext> : View<TRenderContext, TextInputViewModel<TRenderContext>>
    where TRenderContext : IRenderContext
{
    private static readonly Color _backgroundColor = new(28, 30, 34, 255);
    private static readonly Color _labelColor = new(180, 180, 180, 255);
    private static readonly Color _valueColor = new(235, 238, 242, 255);

    private readonly Font _font;

    public TextInputView(
        TextInputViewModel<TRenderContext> viewModel,
        IFontSystem fontSystem)
        : base(viewModel)
    {
        _font = fontSystem.Load("fonts/GohuFont-Medium.ttf", 16);
    }

    public override void Build(Pencil pencil)
    {
        pencil.MoveTo(0, 0);
        pencil.Panel(pencil.BottomRight.X, pencil.BottomRight.Y, _backgroundColor);

        int x = pencil.Center.X - 180;
        int y = 70;
        pencil.MoveTo(x, y);
        pencil.Text(typeof(TRenderContext).Name, _font, _labelColor);

        string text = ViewModel.Text;
        pencil.MoveTo(x, y + 45);
        if (pencil.TextField(0, ref text, _font, 360))
        {
            ViewModel.Text = text;
        }

        pencil.MoveTo(x, y + 100);
        pencil.Text($"This window contains: {ViewModel.Text}", _font, _valueColor);

        pencil.MoveTo(x, y + 145);
        pencil.Text("Click the field and type. Input stays in this window.", _font, _labelColor);
    }
}
