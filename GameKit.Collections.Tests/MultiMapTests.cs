namespace GameKit.Collections.Tests;

public class MultiMapTests
{
    MultiMap<TestKey, int, double> _multiMap;
    [SetUp]
    public void Setup()
    {
        _multiMap = new();
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        _multiMap.Set(new TestKey(42, 1), 42, 21.0);
        
        Assert.That(_multiMap.Contains(new TestKey(42, 1)));
    }

    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse() 
    {
        var key = new TestKey(1 << 16, 0);

        bool result = _multiMap.Contains(key);
        
        Assert.That(!result);
    }

    [Test]
    public void Contains_ForTombStone_ReturnsFalse() 
    {
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        _multiMap.Set(keyToSetInitialSize, 42, 21.0);

        bool result = _multiMap.Contains(key);
        
        Assert.That(!result);
    }

    [Test]
    public void GetKeysIndex_WithMultipleItems_ReturnsCorrectIndex()
    {
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        
        Assert.That(_multiMap.GetKeysIndex(key2), Is.EqualTo(1));
    }

    [Test]
    public void GetKeysIndex_ForDifferentVersions_ReturnsMinusOne() 
    {
        _multiMap.Set(new TestKey(42, 1), 42, 21.0);
        
        Assert.That(_multiMap.GetKeysIndex(new TestKey(42, 0)), Is.EqualTo(-1));
    }

    [Test]
    public void GetKeysIndex_ForUnset_ReturnsMinusOne() 
    {
        _multiMap.Set(new TestKey(42, 0), 42, 21.0);
        
        Assert.That(_multiMap.GetKeysIndex(new TestKey(0, 0)), Is.EqualTo(-1));
    }
    
    [Test]
    public void GetKeysIndex_BeyondItsSize_ReturnsMinusOne() 
    {
        var key = new TestKey(1 << 16, 0);

        Assert.That(_multiMap.GetKeysIndex(key), Is.EqualTo(-1));
    }
    
    [Test]
    public void Contains_ForDifferentVersions_ReturnsFalse() 
    {
        _multiMap.Set(new TestKey(42, 1), 42, 21.0);
        
        Assert.That(!_multiMap.Contains(new TestKey(42, 0)));
        Assert.That(!_multiMap.Contains(new TestKey(42, 2)));
    }
    
    [Test]
    public void Set_AfterSets_KeepsTheOrderOfSets() 
    {
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        
        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key2, key3}));
    }
    
    [Test]
    public void Set_WhenCalledTwiceWithSameKeyAndDifferentValues_ChangesTheValues() 
    {
        var key = new TestKey(42, 0);
        
        _multiMap.Set(key, 42, 21.0);
        _multiMap.Set(key, 43, 22.0);
        
        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {key}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {43}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {22.0}));
    }
    
    [Test]
    public void Set_AfterSetsAndRemove_KeepsExpectedOrder() 
    {
        var key1 = new TestKey(3, 0);
        var key2 = new TestKey(2, 0);
        var key3 = new TestKey(1, 0);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        _multiMap.Remove(key2);

        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key3}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 44}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0, 23.0}));
    }

    [Test]
    public void Set_ForDifferentVersion_UpdatesTheKey() 
    {
        var key1 = new TestKey(3, 0);
        var key2 = new TestKey(2, 0);
        var key3 = new TestKey(1, 0);
        
        var updatedKey2 = new TestKey(2, 3);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        
        _multiMap.Set(updatedKey2, 143, 122.0);
        
        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {key1, updatedKey2, key3}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 143, 44}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0, 122.0, 23.0}));
    }

    [Test]
    public void Remove_WhereSparseArrayHasItemsSet_IgnoresDifferentVersion() 
    {
        var key1 = new TestKey(42, 0);
        var key2 = new TestKey(24, 0);
        var key3 = new TestKey(12, 0);

        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);

        _multiMap.Remove(key2.WithVersion(1));

        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {key1, key2, key3}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 43, 44}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0, 22.0, 23.0}));
    }
    
    [Test]
    public void Remove_BeyondItsSize_RemovesNothing() 
    {
        var key = new TestKey(1 << 16, 0);

        bool result = _multiMap.Remove(key);
        
        Assert.That(!result);
        Assert.That(_multiMap.Keys.ToArray(), Is.Empty);
    }

    [Test]
    public void Remove_ForTombStone_RemovesNothing() 
    {
        var keyToSetInitialSize = new TestKey(42, 0);
        var key = new TestKey(0, 0);
        _multiMap.Set(keyToSetInitialSize, 42, 21.0);

        bool result = _multiMap.Remove(key);
        
        Assert.That(!result);
        Assert.That(_multiMap.Keys.ToArray(), Is.EquivalentTo(new[] {keyToSetInitialSize}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0}));
    }
}
