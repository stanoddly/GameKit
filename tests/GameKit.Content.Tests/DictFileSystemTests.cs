using System.Collections.Frozen;
using System.Collections.Immutable;

namespace GameKit.Content.Tests;

public class DictFileSystemTests: BaseVirtualFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        var files = new Dictionary<string, ImmutableArray<VirtualFile>>
        {
            ["."] = [new ByteVirtualFile("a.txt", "Hello a"u8), new ByteVirtualFile("b.txt", "Hello b"u8)],
            ["dir1"] = [new ByteVirtualFile("dir1/dir1a.txt", "Hello dir1a"u8), new ByteVirtualFile("dir1/dir1b.txt", "Hello dir1b"u8)],
            ["dir2"] = [new ByteVirtualFile("dir2/dir2a.txt", "Hello dir2a"u8), new ByteVirtualFile("dir2/dir2b.txt", "Hello dir2b"u8)],
        }.ToFrozenDictionary();

        var directories = new Dictionary<string, ImmutableArray<string>>
        {
            ["."] = ["dir1", "dir2"],
            ["dir1"] = [],
            ["dir2"] = []
        }.ToFrozenDictionary();

        FileSystem = new DictFileSystem(files, directories);
    }
}