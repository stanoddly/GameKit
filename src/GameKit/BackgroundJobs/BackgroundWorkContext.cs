using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundWorkContext : IBackgroundWorkContext
{
    public ICopyPass CopyPass { get; }

    public BackgroundWorkContext(ICopyPass copyPass)
    {
        CopyPass = copyPass;
    }
}
