namespace GameKit.Collections.Tests;

public class MultiMapTests
{
    MultiMap<int, double> _multiMap;
    [SetUp]
    public void Setup()
    {
        _multiMap = new();
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        _multiMap.Set((Handle)42, 42, 21.0);
        
        Assert.That(_multiMap.Contains((Handle)42));
    }

    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse() 
    {
        Handle handle = (Handle)(1 << 16);

        bool result = _multiMap.Contains(handle);
        
        Assert.That(!result);
    }

    [Test]
    public void Contains_ForTombStone_ReturnsFalse() 
    {
        Handle keyToSetInitialSize = (Handle)(42);
        Handle key = (Handle)(0);
        _multiMap.Set(keyToSetInitialSize, 42, 21.0);

        bool result = _multiMap.Contains(key);
        
        Assert.That(!result);
    }

    [Test]
    public void Contains_WithMultipleItems_ReturnsCorrectIndex()
    {
        var key1 = (Handle)(42);
        var key2 = (Handle)(24);
        var key3 = (Handle)(12);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        
        Assert.That(_multiMap.Contains(key2, out int index), Is.True);
        Assert.That(index, Is.EqualTo(1));
    }

    [Test]
    public void GetKeysIndex_ForUnset_ReturnsMinusOne() 
    {
        _multiMap.Set((Handle)42, 42, 21.0);
        
        Assert.That(_multiMap.Contains((Handle)(0)), Is.False);
    }
    
    [Test]
    public void GetKeysIndex_BeyondItsSize_ReturnsMinusOne() 
    {
        var key = (Handle)(1 << 16);

        Assert.That(_multiMap.Contains(key), Is.False);
    }
    
    [Test]
    public void Set_AfterSets_KeepsTheOrderOfSets() 
    {
        var key1 = (Handle)(42);
        var key2 = (Handle)(24);
        var key3 = (Handle)(12);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        
        Assert.That(_multiMap.Handles.ToArray(), Is.EquivalentTo(new[] {key1, key2, key3}));
    }
    
    [Test]
    public void Set_WhenCalledTwiceWithSameKeyAndDifferentValues_ChangesTheValues() 
    {
        var key = (Handle)(42);
        
        _multiMap.Set(key, 42, 21.0);
        _multiMap.Set(key, 43, 22.0);
        
        Assert.That(_multiMap.Handles.ToArray(), Is.EquivalentTo(new[] {key}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {43}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {22.0}));
    }
    
    [Test]
    public void Set_AfterSetsAndRemove_KeepsExpectedOrder() 
    {
        var key1 = (Handle)(3);
        var key2 = (Handle)(2);
        var key3 = (Handle)(1);
        
        _multiMap.Set(key1, 42, 21.0);
        _multiMap.Set(key2, 43, 22.0);
        _multiMap.Set(key3, 44, 23.0);
        _multiMap.Remove(key2);

        Assert.That(_multiMap.Handles.ToArray(), Is.EquivalentTo(new[] {key1, key3}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 44}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0, 23.0}));
    }
    
    [Test]
    public void Remove_BeyondItsSize_RemovesNothing() 
    {
        var key = (Handle)(1 << 16);

        bool result = _multiMap.Remove(key);
        
        Assert.That(!result);
        Assert.That(_multiMap.Handles.ToArray(), Is.Empty);
    }

    [Test]
    public void Remove_ForTombStone_RemovesNothing() 
    {
        var keyToSetInitialSize = (Handle)(42);
        var key = (Handle)(0);
        _multiMap.Set(keyToSetInitialSize, 42, 21.0);

        bool result = _multiMap.Remove(key);
        
        Assert.That(!result);
        Assert.That(_multiMap.Handles.ToArray(), Is.EquivalentTo(new[] {keyToSetInitialSize}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42}));
        Assert.That(_multiMap.Values2.ToArray(), Is.EquivalentTo(new[] {21.0}));
    }

    [Test]
    public void RemoveInTheMiddleRearrangesValuesAsExpected()
    {
        // Arrange
        Handle handle1 = (Handle)0;
        Handle handle2 = (Handle)1;
        Handle handle3 = (Handle)2;
        Handle handle4 = (Handle)3;

        _multiMap.Set(handle1, 42, 0);
        _multiMap.Set(handle2, 43, 0);
        _multiMap.Set(handle3, 44, 0);
        _multiMap.Set(handle4, 45, 0);

        // Act
        _multiMap.Remove(handle2);

        // Assert
        int value;
        bool contains;

        contains = _multiMap.TryGetValue1(handle1, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(42));
        
        contains = _multiMap.TryGetValue1(handle2, out value);
        Assert.That(contains, Is.False);

        contains = _multiMap.TryGetValue1(handle3, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(44));
        
        contains = _multiMap.TryGetValue1(handle4, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(45));
        
        Assert.That(_multiMap.Handles.ToArray(), Is.EquivalentTo(new[] {handle1, handle4, handle3}));
        Assert.That(_multiMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 45, 44}));
    }
}
