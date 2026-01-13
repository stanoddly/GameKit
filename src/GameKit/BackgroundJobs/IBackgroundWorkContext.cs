using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Provides resources for background work processing, such as GPU access.
/// </summary>
public interface IBackgroundWorkContext
{
    /// <summary>
    /// Gets the copy pass for GPU memory transfers.
    /// </summary>
    ICopyPass CopyPass { get; }
}
