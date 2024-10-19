using GameKit.Content;
using Assert = NUnit.Framework.Assert;
using System.Linq;

namespace GameKit.Tests;

public class NativeFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void Test1()
    {
        using NativeFileSystem nativeFileSystem = new NativeFileSystem("Content");
        
        var files = nativeFileSystem.GetFiles(".");

        string[] expected = ["a.txt", "b.txt"];
        VirtualFile[] items = files.ToArray();
        
        CollectionAssert.AreEquivalent(expected, items.Select(x => x.Path));
    }
    
    [Test]
    public void Test2()
    {
        using NativeFileSystem nativeFileSystem = new NativeFileSystem("Content");
        
        var files = nativeFileSystem.GetFiles("dir1");

        string[] expected = ["dir1a.txt", "dir1b.txt"];
        VirtualFile[] items = files.ToArray();
        
        CollectionAssert.AreEquivalent(expected, items.Select(x => x.Path));
    }
}