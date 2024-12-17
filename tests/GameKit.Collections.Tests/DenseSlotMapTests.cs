namespace GameKit.Collections.Tests;

public class DenseSlotMapTests
{
    DenseSlotMap<Handle<object>, int> _slotMap;

    [SetUp]
    public void Setup()
    {
        _slotMap = new();
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        Handle<object> handle = _slotMap.Add(42);
        
        Assert.That(_slotMap.Contains(handle));
    }

    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse()
    {
        Handle<object> handle = new Handle<object>{ Index = 1234, Version = 0 };
        bool result = _slotMap.Contains(handle);
        
        Assert.That(!result);
    }
    
    [Test]
    public void AddAfterRemovalRecyclesIndex()
    {
        // Arrange
        Handle<object> handleFirst = _slotMap.Add(42);
        _slotMap.Remove(handleFirst);
        
        // Act
        Handle<object> handleSecond = _slotMap.Add(43);
        
        // Assert
        Assert.That(handleFirst.Index == handleSecond.Index);
        Assert.That(handleFirst.Version != handleSecond.Version);
    }
    
    [Test]
    public void AddAfterRemovalReplacesTheOldValue()
    {
        // Arrange
        Handle<object> handleFirst = _slotMap.Add(42);
        _slotMap.Remove(handleFirst);
        
        // Act
        Handle<object> handleSecond = _slotMap.Add(43);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue1(handleFirst, out value);
        Assert.That(contains, Is.False);
        
        contains = _slotMap.TryGetValue1(handleSecond, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(43));
    }
    
    [Test]
    public void RemoveInTheMiddleRearrangesValuesAsExpected()
    {
        // Arrange
        Handle<object> handle1 = _slotMap.Add(42);
        Handle<object> handle2 = _slotMap.Add(43);
        Handle<object> handle3 = _slotMap.Add(44);
        Handle<object> handle4 = _slotMap.Add(45);

        // Act
        _slotMap.Remove(handle2);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue1(handle1, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(42));
        
        contains = _slotMap.TryGetValue1(handle2, out value);
        Assert.That(contains, Is.False);

        contains = _slotMap.TryGetValue1(handle3, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(44));
        
        contains = _slotMap.TryGetValue1(handle4, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(45));
        
        Assert.That(_slotMap.Handles.ToArray(), Is.EquivalentTo(new[] {handle1, handle4, handle3}));
        Assert.That(_slotMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 45, 44}));
    }
    
    [Test]
    public void ThreeAddAfterTwoRemovalsWorkAsExpected()
    {
        // Act
        Handle<object> handle1 = _slotMap.Add(42);
        Handle<object> handle2 = _slotMap.Add(43);
        
        _slotMap.Remove(handle2);
        _slotMap.Remove(handle1);
        
        Handle<object> handle3 = _slotMap.Add(52);
        Handle<object> handle4 = _slotMap.Add(53);
        Handle<object> handle5 = _slotMap.Add(54);

        // Assert
        bool contains;

        contains = _slotMap.TryGetValue1(handle1, out _);
        Assert.That(contains, Is.False);
        
        contains = _slotMap.TryGetValue1(handle2, out _);
        Assert.That(contains, Is.False);
        
        Assert.That(_slotMap.Handles.ToArray(), Is.EquivalentTo(new[] {handle3, handle4, handle5}));
        Assert.That(_slotMap.Values1.ToArray(), Is.EquivalentTo(new[] {52, 53, 54}));
    }
    
    [Test]
    public void AddAfterRemoveInTheMiddleRecyclesIndex()
    {
        // Arrange
        Handle<object> handle1 = _slotMap.Add(42);
        Handle<object> handle2 = _slotMap.Add(43);
        Handle<object> handle3 = _slotMap.Add(44);
        Handle<object> handle4 = _slotMap.Add(45);
        
        _slotMap.Remove(handle2);

        // Act
        Handle<object> handle5 = _slotMap.Add(53);

        // Assert
        int value;
        bool contains;
        
        contains = _slotMap.TryGetValue1(handle2, out value);
        Assert.That(contains, Is.False);
        
        contains = _slotMap.TryGetValue1(handle5, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(53));
        
        Assert.That(handle2.Index, Is.EqualTo(handle5.Index));
        Assert.That(handle2.Version, Is.Not.EqualTo(handle5.Index));
        
        Assert.That(_slotMap.Handles.ToArray(), Is.EquivalentTo(new[] {handle1, handle4, handle3, handle5}));
        Assert.That(_slotMap.Values1.ToArray(), Is.EquivalentTo(new[] {42, 45, 44, 53}));
    }

    [Test]
    public void Add_MultipleTimes_WorksAsExpected()
    {
        // Act
        Handle<object> handle1 = _slotMap.Add(42);
        Handle<object> handle2 = _slotMap.Add(43);
        Handle<object> handle3 = _slotMap.Add(44);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue1(handle1, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(42));

        contains = _slotMap.TryGetValue1(handle2, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(43));

        contains = _slotMap.TryGetValue1(handle3, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(44));
    }
}
