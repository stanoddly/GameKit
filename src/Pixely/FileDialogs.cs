namespace Pixely;

public enum FileDialogStatus
{
    Accepted,
    Canceled,
    Error
}

public readonly record struct FileDialogFilter(string Name, string Pattern);

public sealed record FileDialogResult(
    FileDialogStatus Status,
    IReadOnlyList<string> Paths,
    string? Error = null)
{
    internal static FileDialogResult Accepted(IReadOnlyList<string> paths)
    {
        return new FileDialogResult(FileDialogStatus.Accepted, paths);
    }

    internal static FileDialogResult Canceled()
    {
        return new FileDialogResult(FileDialogStatus.Canceled, Array.Empty<string>());
    }

    internal static FileDialogResult Failed(string error)
    {
        return new FileDialogResult(FileDialogStatus.Error, Array.Empty<string>(), error);
    }
}
