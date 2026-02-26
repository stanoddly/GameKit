namespace GameKit.Collections.Tests;

public class SlotSetTests
{
    SlotSet slotSet;

    [SetUp]
    public void Setup()
    {
        slotSet = new();
    }

    [Test]
    public void Contains_AfterSet_ReturnsTrue()
    {
        var handle = (Handle)5;

        slotSet.Set(handle);

        Assert.That(slotSet.Contains(handle));
    }

    [Test]
    public void Contains_ForUnsetHandle_ReturnsFalse()
    {
        slotSet.Set((Handle)5);

        Assert.That(!slotSet.Contains((Handle)10));
    }

    [Test]
    public void Contains_ForOutOfBoundsHandle_ReturnsFalse()
    {
        Assert.That(!slotSet.Contains((Handle)999));
    }

    [Test]
    public void Length_AfterMultipleSets_ReturnsCorrectCount()
    {
        slotSet.Set((Handle)1);
        slotSet.Set((Handle)5);
        slotSet.Set((Handle)10);

        Assert.That(slotSet.Length, Is.EqualTo(3));
    }

    [Test]
    public void Remove_AfterSet_HandleNoLongerContained()
    {
        var handle = (Handle)5;
        slotSet.Set(handle);

        bool removed = slotSet.Remove(handle);

        Assert.That(removed);
        Assert.That(!slotSet.Contains(handle));
        Assert.That(slotSet.Length, Is.EqualTo(0));
    }

    [Test]
    public void Remove_FromMiddle_SwapRemovesCorrectly()
    {
        var h1 = (Handle)1;
        var h2 = (Handle)2;
        var h3 = (Handle)3;
        slotSet.Set(h1);
        slotSet.Set(h2);
        slotSet.Set(h3);

        slotSet.Remove(h2);

        Assert.That(slotSet.Length, Is.EqualTo(2));
        Assert.That(slotSet.Contains(h1));
        Assert.That(!slotSet.Contains(h2));
        Assert.That(slotSet.Contains(h3));
        Assert.That(slotSet.Handles.ToArray(), Is.EquivalentTo(new[] { h1, h3 }));
    }

    [Test]
    public void Set_SameHandleTwice_IsIdempotent()
    {
        var handle = (Handle)5;

        slotSet.Set(handle);
        slotSet.Set(handle);

        Assert.That(slotSet.Length, Is.EqualTo(1));
    }

    [Test]
    public void Handles_AfterMultipleSets_ReturnsCorrectValues()
    {
        var h1 = (Handle)10;
        var h2 = (Handle)20;
        var h3 = (Handle)30;
        slotSet.Set(h1);
        slotSet.Set(h2);
        slotSet.Set(h3);

        Assert.That(slotSet.Handles.ToArray(), Is.EquivalentTo(new[] { h1, h2, h3 }));
    }

    [Test]
    public void Remove_OutOfBoundsHandle_ReturnsFalse()
    {
        bool removed = slotSet.Remove((Handle)999);

        Assert.That(!removed);
    }
}

public class SlotSetStructTests
{
    SlotSetStruct slotSet;

    [SetUp]
    public void Setup()
    {
        slotSet = new();
    }

    [Test]
    public void Contains_AfterSet_ReturnsTrue()
    {
        var handle = (Handle)5;

        slotSet.Set(handle);

        Assert.That(slotSet.Contains(handle));
    }

    [Test]
    public void Contains_ForUnsetHandle_ReturnsFalse()
    {
        slotSet.Set((Handle)5);

        Assert.That(!slotSet.Contains((Handle)10));
    }

    [Test]
    public void Remove_FromMiddle_SwapRemovesCorrectly()
    {
        var h1 = (Handle)1;
        var h2 = (Handle)2;
        var h3 = (Handle)3;
        slotSet.Set(h1);
        slotSet.Set(h2);
        slotSet.Set(h3);

        slotSet.Remove(h2);

        Assert.That(slotSet.Length, Is.EqualTo(2));
        Assert.That(slotSet.Contains(h1));
        Assert.That(!slotSet.Contains(h2));
        Assert.That(slotSet.Contains(h3));
        Assert.That(slotSet.Handles.ToArray(), Is.EquivalentTo(new[] { h1, h3 }));
    }

    [Test]
    public void Set_SameHandleTwice_IsIdempotent()
    {
        var handle = (Handle)5;

        slotSet.Set(handle);
        slotSet.Set(handle);

        Assert.That(slotSet.Length, Is.EqualTo(1));
    }
}
