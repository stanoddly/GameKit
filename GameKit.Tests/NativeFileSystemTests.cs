using GameKit.Content;
using Assert = NUnit.Framework.Assert;
using System.Linq;

namespace GameKit.Tests;

public class NativeFileSystemTests
{
    private NativeFileSystem _nativeFileSystem = null!;
    [SetUp]
    public void Setup()
    {
        _nativeFileSystem = new NativeFileSystem("Content");
    }

    [Test]
    public void Test1()
    {
        var files = _nativeFileSystem.GetFiles(".");

        string[] expected = ["a.txt", "b.txt"];
        ContentFile[] items = files.ToArray();
        
        CollectionAssert.AreEquivalent(expected, items.Select(x => x.Path));
    }

    [TearDown]
    public void TearDown()
    {
        _nativeFileSystem.Dispose();
    }
}