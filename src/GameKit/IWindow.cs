using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;

namespace GameKit;

public readonly record struct ResolutionChangedEventArgs(ShortSize OldSize, ShortSize NewSize, ulong Timestamp);

public delegate void ResolutionChangedHandler(ResolutionChangedEventArgs eventArgs);

public interface IWindow : IDisposable
{
    uint Id { get; }

    ShortSize RenderSizeInPixels { get; }

    TextureFormat ColorTargetFormat { get; }
    bool MouseGrab { get; set; }
    bool WindowRelativeMouseMode { get; set; }

    event ResolutionChangedHandler? ResolutionChanged;

    bool TryAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture);

    void SetFullscreenBorderless(bool fullscreen);

    void SetIcon(Image icon);

    /// <summary>
    /// Shows a native modal file-open dialog and blocks GameKit execution until the dialog completes.
    /// Intended for editor and tooling workflows. While this method is running, GameKit does not update,
    /// render, process input services, or advance timers.
    /// </summary>
    FileDialogResult ShowModalOpenFileDialog(IReadOnlyList<FileDialogFilter>? filters = null, string? defaultLocation = null, bool allowMany = false);

    /// <summary>
    /// Shows a native modal file-save dialog and blocks GameKit execution until the dialog completes.
    /// Intended for editor and tooling workflows. While this method is running, GameKit does not update,
    /// render, process input services, or advance timers.
    /// </summary>
    FileDialogResult ShowModalSaveFileDialog(IReadOnlyList<FileDialogFilter>? filters = null, string? defaultLocation = null);
}
