namespace GameKit.Collections.Tests;

public class SlotMapStructTests
{
    SlotMapStruct<Handle64<object>> _slotMap;

    [SetUp]
    public void Setup()
    {
        _slotMap = new();
    }

    [Test]
    public void CreateHandle_WhenCalled_ReturnsNonNullHandle()
    {
        Handle64<object> handle = _slotMap.CreateHandle();

        Assert.That(handle.IsNull(), Is.False);
    }

    [Test]
    public void CreateHandle_CalledTwice_ReturnsUniqueHandles()
    {
        Handle64<object> first = _slotMap.CreateHandle();
        Handle64<object> second = _slotMap.CreateHandle();

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void Contains_WithCreatedHandle_ReturnsTrue()
    {
        Handle64<object> handle = _slotMap.CreateHandle();

        Assert.That(_slotMap.Contains(handle), Is.True);
    }

    [Test]
    public void Contains_WithNonExistentHandle_ReturnsFalse()
    {
        Handle64<object> handle = new Handle64<object> { Index = 999, Version = 0 };

        Assert.That(_slotMap.Contains(handle), Is.False);
    }

    [Test]
    public void Contains_AfterRemove_ReturnsFalse()
    {
        Handle64<object> handle = _slotMap.CreateHandle();
        _slotMap.Remove(handle);

        Assert.That(_slotMap.Contains(handle), Is.False);
    }

    [Test]
    public void Remove_AndCreateAgain_RecyclesIndexWithNewVersion()
    {
        Handle64<object> first = _slotMap.CreateHandle();
        _slotMap.Remove(first);

        Handle64<object> second = _slotMap.CreateHandle();

        Assert.That(second.Index, Is.EqualTo(first.Index));
        Assert.That(second.Version, Is.Not.EqualTo(first.Version));
    }

    [Test]
    public void Contains_WithStaleHandle_ReturnsFalse()
    {
        Handle64<object> original = _slotMap.CreateHandle();
        _slotMap.Remove(original);
        _slotMap.CreateHandle(); // recycles the slot

        Assert.That(_slotMap.Contains(original), Is.False);
    }

    [Test]
    public void Count_ReflectsActiveHandles()
    {
        Assert.That(_slotMap.Count, Is.EqualTo(0));

        Handle64<object> h1 = _slotMap.CreateHandle();
        Handle64<object> h2 = _slotMap.CreateHandle();
        Assert.That(_slotMap.Count, Is.EqualTo(2));

        _slotMap.Remove(h1);
        Assert.That(_slotMap.Count, Is.EqualTo(1));

        _slotMap.Remove(h2);
        Assert.That(_slotMap.Count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_WithNullHandle_ReturnsFalse()
    {
        bool result = _slotMap.Remove(Handle64<object>.Null);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Remove_SameHandleTwice_ReturnsFalseOnSecondCall()
    {
        Handle64<object> handle = _slotMap.CreateHandle();

        Assert.That(_slotMap.Remove(handle), Is.True);
        Assert.That(_slotMap.Remove(handle), Is.False);
    }
}
