namespace GameKit.Collections.Tests;

public class SparseSetTests
{
    SparseSet<TestKey> sparseSet;

    [SetUp]
    public void Setup()
    {
        sparseSet = new();
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(sparseSet.Contains(new TestKey(42, 1)));
    }
    
    [Test]
    public void GetKeysIndex_WithMultipleItems_ReturnsCorrectIndex()
    {
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
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(sparseSet.GetKeysIndex(new TestKey(42, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void GetKeysIndex_ForUnset_ReturnsMinusOne() 
    {
        sparseSet.Set(new TestKey(42, 0));
        
        Assert.That(sparseSet.GetKeysIndex(new TestKey(0, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void GetKeysIndex_BeyondItsSize_ReturnsMinusOne() 
    {
        var key = new TestKey(1 << 16, 0);
        
        Assert.That(sparseSet.GetKeysIndex(key), Is.EqualTo(-1));
    }
    
    [Test]
    public void Contains_ForDifferentVersions_ReturnsFalse() 
    {
        sparseSet.Set(new TestKey(42, 1));
        
        Assert.That(!sparseSet.Contains(new TestKey(42, 0)));
        Assert.That(!sparseSet.Contains(new TestKey(42, 2)));
    }
    
    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse() 
    {
        var key = new TestKey(1 << 16, 0);

        bool result = sparseSet.Contains(key);
        
        Assert.That(!result);
    }
    
    [Test]
    public void Contains_ForTombStone_ReturnsFalse() 
    {
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        sparseSet.Set(keyToSetInitialSize);

        bool result = sparseSet.Contains(key);
        
        Assert.That(!result);
    }
    
    [Test]
    public void Set_AfterSets_KeepsTheOrderOfSets() 
    {
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
        var key = new TestKey(42, 0);
        
        sparseSet.Set(key);
        sparseSet.Set(key);
        
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {key}));
    }
    
    [Test]
    public void Set_AfterSetsAndRemove_KeepsExpectedOrder() 
    {
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
        var key = new TestKey(1 << 16, 0);

        SwapRemoveResult result = sparseSet.SwapRemove(key);
        
        Assert.That(result == SwapRemoveResult.Absent);
        Assert.That(sparseSet.Keys.ToArray(), Is.Empty);
    }
    
    [Test]
    public void SwapRemove_ForTombStone_RemovesNothing() 
    {
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        sparseSet.Set(keyToSetInitialSize);

        SwapRemoveResult result = sparseSet.SwapRemove(key);
        
        Assert.That(result == SwapRemoveResult.Absent);
        Assert.That(sparseSet.Keys.ToArray(), Is.EquivalentTo(new[] {keyToSetInitialSize}));
    }
}
