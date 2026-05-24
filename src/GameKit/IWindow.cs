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

    /// <summary>
    /// True when the active SDL video backend is expected to support always-on-top windows.
    /// </summary>
    bool SupportsAlwaysOnTop { get; }

    /// <summary>
    /// Requests that the window stays above other windows. Some platforms or compositors may ignore this.
    /// </summary>
    bool AlwaysOnTop { get; set; }

    /// <summary>
    /// Gets or sets the position of the window on the display, in screen coordinates.
    /// </summary>
    GameKit.Common.Vector2Int Position { get; set; }

    event ResolutionChangedHandler? ResolutionChanged;

    bool TryWaitAndAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture);

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
