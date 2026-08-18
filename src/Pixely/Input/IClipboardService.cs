namespace Pixely.Input;

public interface IClipboardService
{
    bool HasText { get; }
    string? GetText();
    void SetText(string text);
}
