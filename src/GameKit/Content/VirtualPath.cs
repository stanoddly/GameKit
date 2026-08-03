namespace GameKit.Content;

internal static class VirtualPath
{
    private const char DirectorySeparator = '/';

    public static string Combine(string first, string second)
    {
        if (first.Length == 0)
        {
            return second;
        }

        if (second.Length == 0)
        {
            return first;
        }

        bool firstEndsWithSeparator = first[^1] == DirectorySeparator;
        bool secondStartsWithSeparator = second[0] == DirectorySeparator;

        if (firstEndsWithSeparator && secondStartsWithSeparator)
        {
            return string.Concat(first, second.AsSpan(1));
        }

        if (firstEndsWithSeparator || secondStartsWithSeparator)
        {
            return string.Concat(first, second);
        }

        return string.Concat(first, DirectorySeparator, second);
    }

    public static string? GetDirectoryName(string path)
    {
        int separatorIndex = path.LastIndexOf(DirectorySeparator);
        return separatorIndex < 0 ? null : path[..separatorIndex];
    }

    public static string GetFileName(string path)
    {
        int separatorIndex = path.LastIndexOf(DirectorySeparator);
        return path[(separatorIndex + 1)..];
    }
}
