namespace GameKit.Collections.Tests;

internal readonly struct TestKey: IKey<TestKey>
{
    public TestKey(int index, int version)
    {
        Index = index;
        Version = version;
    }

    public int Index { get; }
    public int Version { get; }

    public static TestKey TombStone { get; } = new TestKey(int.MaxValue, int.MaxValue);
    public bool IsTombStone()
    {
        return Index == int.MaxValue;
    }

    public TestKey WithIndex(int index)
    {
        return new TestKey(index, Version);
    }

    public TestKey WithVersion(int version)
    {
        return new TestKey(Index, version);
    }
}

public class SparseSetTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        SparseSet<TestKey> sparseSet = new();
        
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(sparseSet.Contains(new TestKey(42, 1)));
    }
    
    [Test]
    public void GetKeysIndex_WithMultipleItems_ReturnsCorrectIndex()
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        sparseSet.Set(key1);
        sparseSet.Set(key2);
        sparseSet.Set(key3);
        
        Assert.That(sparseSet.GetKeysIndex(key2), Is.EqualTo(1));
    }
    
    [Test]
    public void GetKeysIndex_ForDifferentVersions_ReturnsMinusOne() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(sparseSet.GetKeysIndex(new TestKey(42, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void GetKeysIndex_ForUnset_ReturnsMinusOne() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        sparseSet.Set(new TestKey(42, 0));
        
        Assert.That(sparseSet.GetKeysIndex(new TestKey(0, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void GetKeysIndex_BeyondItsSize_ReturnsMinusOne() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key = new TestKey(1 << 16, 0);

        bool result = sparseSet.Contains(key);
        
        Assert.That(sparseSet.GetKeysIndex(new TestKey(0, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void Contains_ForDifferentVersions_ReturnsFalse() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(!sparseSet.Contains(new TestKey(42, 0)));
        Assert.That(!sparseSet.Contains(new TestKey(42, 2)));
    }
    
    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key = new TestKey(1 << 16, 0);

        bool result = sparseSet.Contains(key);
        
        Assert.That(!result);
    }
    
    [Test]
    public void Contains_ForTombStone_ReturnsFalse() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        sparseSet.Set(keyToSetInitialSize);

        bool result = sparseSet.Contains(key);
        
        Assert.That(!result);
    }
    
    [Test]
    public void Set_AfterSets_KeepsTheOrderOfSets() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        sparseSet.Set(key1);
        sparseSet.Set(key2);
        sparseSet.Set(key3);
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key2, key3}));
    }
    
    [Test]
    public void Set_WhenCalledTwice_DoesNothing() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key = new TestKey(42, 0);
        
        sparseSet.Set(key);
        sparseSet.Set(key);
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key}));
    }
    
    [Test]
    public void Set_AfterSetsAndRemove_KeepsExpectedOrder() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        sparseSet.Set(key1);
        sparseSet.Set(key2);
        sparseSet.Set(key3);
        sparseSet.SwapRemove(key2);
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key3}));
    }
    
    [Test]
    public void Set_ForDifferentVersion_UpdatesTheKey() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        var updatedKey2 = new TestKey(24, 3);
        
        sparseSet.Set(key1);
        sparseSet.Set(key2);
        sparseSet.Set(key3);
        
        sparseSet.Set(updatedKey2);
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key1, updatedKey2, key3}));
    }
    
    [Test]
    public void SwapRemove_WhereSparseArrayHasItemsSet_IgnoresDifferentVersion() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        sparseSet.Set(key1);
        sparseSet.Set(key2);
        sparseSet.Set(key3);
        
        sparseSet.SwapRemove(key2.WithVersion(1));
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key2, key3}));
    }
    
    [Test]
    public void SwapRemove_BeyondItsSize_RemovesNothing() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var key = new TestKey(1 << 16, 0);

        SwapRemoveResult result = sparseSet.SwapRemove(key);
        
        Assert.That(result == SwapRemoveResult.Absent);
        Assert.That(sparseSet.Keys.ToArray(), Is.Empty);
    }
    
    [Test]
    public void SwapRemove_ForTombStone_RemovesNothing() 
    {
        SparseSet<TestKey> sparseSet = new();
        
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        sparseSet.Set(keyToSetInitialSize);

        SwapRemoveResult result = sparseSet.SwapRemove(key);
        
        Assert.That(result == SwapRemoveResult.Absent);
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {keyToSetInitialSize}));
    }
}
