namespace GameKit;

public enum FileDialogStatus
{
    Accepted,
    Canceled,
    Error
}

public readonly record struct FileDialogFilter(string Name, string Pattern);

public sealed record OpenFileDialogOptions(
    IReadOnlyList<FileDialogFilter> Filters,
    string? DefaultLocation = null,
    bool AllowMany = false);

public sealed record SaveFileDialogOptions(
    IReadOnlyList<FileDialogFilter> Filters,
    string? DefaultLocation = null);

public sealed record FileDialogResult(
    FileDialogStatus Status,
    IReadOnlyList<string> Paths,
    string? Error = null)
{
    public static FileDialogResult Accepted(IReadOnlyList<string> paths)
    {
        return new FileDialogResult(FileDialogStatus.Accepted, paths);
    }

    public static FileDialogResult Canceled()
    {
        return new FileDialogResult(FileDialogStatus.Canceled, Array.Empty<string>());
    }

    public static FileDialogResult Failed(string error)
    {
        return new FileDialogResult(FileDialogStatus.Error, Array.Empty<string>(), error);
    }
}
