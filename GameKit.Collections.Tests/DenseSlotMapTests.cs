namespace GameKit.Collections.Tests;

public class DenseSlotMapTests
{
    DenseSlotMap<int> _slotMap;

    [SetUp]
    public void Setup()
    {
        _slotMap = new();
    }

    [Test]
    public void Contains_WithOneItem_ReturnsTrue()
    {
        Handle handle = _slotMap.Add(42);
        
        Assert.That(_slotMap.Contains(handle));
    }

    [Test]
    public void Contains_BeyondItsSize_ReturnsFalse()
    {
        Handle handle = new Handle(1234, 0);
        bool result = _slotMap.Contains(handle);
        
        Assert.That(!result);
    }
    
    [Test]
    public void Add_AfterRemoval_RecyclesIndex()
    {
        // Arrange
        Handle handleFirst = _slotMap.Add(42);
        _slotMap.Remove(handleFirst);
        
        // Act
        Handle handleSecond = _slotMap.Add(43);
        
        // Assert
        Assert.That(handleFirst.Index == handleSecond.Index);
        Assert.That(handleFirst.Version != handleSecond.Version);
    }
    
    [Test]
    public void Add_AfterRemoval_ReplacesTheOldValue()
    {
        // Arrange
        Handle handleFirst = _slotMap.Add(42);
        _slotMap.Remove(handleFirst);
        
        // Act
        Handle handleSecond = _slotMap.Add(43);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue(handleFirst, out value);
        Assert.That(contains, Is.False);
        
        contains = _slotMap.TryGetValue(handleSecond, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(43));
    }
    
    [Test]
    public void Remove_OfTheFirstValue_RearrangesValuesAsExpected()
    {
        // Arrange
        Handle handle1 = _slotMap.Add(42);
        Handle handle2 = _slotMap.Add(43);
        Handle handle3 = _slotMap.Add(44);
        Handle handle4 = _slotMap.Add(45);

        // Act
        _slotMap.Remove(handle2);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue(handle1, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(42));
        
        contains = _slotMap.TryGetValue(handle2, out value);
        Assert.That(contains, Is.False);

        contains = _slotMap.TryGetValue(handle3, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(44));
        
        contains = _slotMap.TryGetValue(handle4, out value);
        Assert.That(contains);
        Assert.That(value, Is.EqualTo(45));
        
        Assert.That(_slotMap.Handles.ToArray(), Is.EquivalentTo(new[] {handle1, handle4, handle3}));
        Assert.That(_slotMap.Values.ToArray(), Is.EquivalentTo(new[] {42, 45, 44}));
    }

    [Test]
    public void Add_MultipleTimes_WorksAsExpected()
    {
        // Act
        Handle handle1 = _slotMap.Add(42);
        Handle handle2 = _slotMap.Add(43);
        Handle handle3 = _slotMap.Add(44);

        // Assert
        int value;
        bool contains;

        contains = _slotMap.TryGetValue(handle1, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(42));

        contains = _slotMap.TryGetValue(handle2, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(43));

        contains = _slotMap.TryGetValue(handle3, out value);
        Assert.That(contains, Is.True);
        Assert.That(value, Is.EqualTo(44));
    }
}
