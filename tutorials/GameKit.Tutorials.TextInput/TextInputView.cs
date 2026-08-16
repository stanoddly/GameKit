using GameKit.Gpu;
using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tutorials.TextInput;

public class TextInputViewModel : IPencuilViewModel
{
    private readonly IClipboardService _clipboardService;

    public bool IsDirty { get; set; } = true;

    private string _name = "Player";
    private string _width = "64";
    private string _height = "48";

    public TextInputViewModel(IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                IsDirty = true;
            }
        }
    }

    public string Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                IsDirty = true;
            }
        }
    }

    public string Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                IsDirty = true;
            }
        }
    }

    public string ClipboardText => _clipboardService.HasText ? (_clipboardService.GetText() ?? "") : "";
}

public class TextInputView : PencuilView<TextInputViewModel>
{
    private static readonly Color BackgroundColor = new(28, 30, 34, 255);
    private static readonly Color LabelColor = new(180, 180, 180, 255);
    private static readonly Color ValueColor = new(235, 238, 242, 255);

    private readonly Font _font;
    private readonly Font _labelFont;

    public TextInputView(TextInputViewModel viewModel, IFontSystem fontSystem)
        : base(viewModel)
    {
        _font = fontSystem.Load("fonts/GohuFont-Medium.ttf", 16);
        _labelFont = fontSystem.Load("fonts/GohuFont-Medium.ttf", 14);
    }

    public override void Build(Pencil pencil)
    {
        pencil.MoveTo(0, 0);
        pencil.Panel(pencil.BottomRight.X, pencil.BottomRight.Y, BackgroundColor);

        int startX = pencil.Center.X - 120;
        int startY = 80;

        using (pencil.WithGap(12))
        using (pencil.WithDirection(LayoutDirection.Bottom))
        {
            pencil.MoveTo(startX, startY);

            pencil.Text("Name", _labelFont, LabelColor);

            string name = ViewModel.Name;
            if (pencil.TextField(0, ref name, _font, 240))
            {
                ViewModel.Name = name;
            }

            pencil.Text("Width", _labelFont, LabelColor);

            string width = ViewModel.Width;
            if (pencil.TextField(1, ref width, _font, 240))
            {
                ViewModel.Width = width;
            }

            pencil.Text("Height", _labelFont, LabelColor);

            string height = ViewModel.Height;
            if (pencil.TextField(2, ref height, _font, 240))
            {
                ViewModel.Height = height;
            }
        }

        pencil.MoveTo(startX, startY + 280);
        pencil.Text($"Name: {ViewModel.Name}  Size: {ViewModel.Width}x{ViewModel.Height}", _font, ValueColor);

        string clipboardText = ViewModel.ClipboardText;
        if (clipboardText.Length > 0)
        {
            pencil.MoveTo(startX, startY + 310);
            pencil.Text($"Clipboard: {clipboardText}", _labelFont, LabelColor);
        }
    }
}
