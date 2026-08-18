namespace Pixely.Collections.Tests;

public class FastListTests
{
    FastList<int> list;

    [SetUp]
    public void Setup()
    {
        list = new();
    }

    [Test]
    public void Add_SingleItem_LengthIsOne()
    {
        list.Add(42);

        Assert.That(list.Length, Is.EqualTo(1));
    }

    [Test]
    public void Add_MultipleItems_LengthIncreases()
    {
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.Length, Is.EqualTo(3));
    }

    [Test]
    public void Add_MultipleItems_ValuesAccessibleViaIndexer()
    {
        list.Add(10);
        list.Add(20);
        list.Add(30);

        Assert.That(list[0], Is.EqualTo(10));
        Assert.That(list[1], Is.EqualTo(20));
        Assert.That(list[2], Is.EqualTo(30));
    }

    [Test]
    public void Add_ReturnsIndex()
    {
        int idx0 = list.Add(10);
        int idx1 = list.Add(20);

        Assert.That(idx0, Is.EqualTo(0));
        Assert.That(idx1, Is.EqualTo(1));
    }

    [Test]
    public void Add_BeyondInitialCapacity_GrowsCorrectly()
    {
        var smallList = new FastList<int>(4);

        for (int i = 0; i < 10; i++)
        {
            smallList.Add(i * 100);
        }

        Assert.That(smallList.Length, Is.EqualTo(10));
        for (int i = 0; i < 10; i++)
        {
            Assert.That(smallList[i], Is.EqualTo(i * 100));
        }
    }

    [Test]
    public void Resize_ToLargerLength_PreservesExistingValues()
    {
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Resize(6);

        Assert.That(list.Length, Is.EqualTo(6));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(3));
    }

    [Test]
    public void Resize_ToSmallerLength_TruncatesLength()
    {
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Resize(1);

        Assert.That(list.Length, Is.EqualTo(1));
        Assert.That(list[0], Is.EqualTo(1));
    }

    [Test]
    public void ResizeFill_WithDefaultValue_FillsNewSlots()
    {
        list.Add(1);
        list.Add(2);

        list.ResizeFill(5, -1);

        Assert.That(list.Length, Is.EqualTo(5));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(-1));
        Assert.That(list[3], Is.EqualTo(-1));
        Assert.That(list[4], Is.EqualTo(-1));
    }

    [Test]
    public void ResizeFill_WithFactory_FillsAllNewSlots()
    {
        list.Add(1);
        list.Add(2);

        int counter = 0;
        list.ResizeFill(5, () => ++counter);

        Assert.That(list.Length, Is.EqualTo(5));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        // The factory should have been called 3 times for indices 2, 3, 4
        Assert.That(list[2], Is.EqualTo(1));
        Assert.That(list[3], Is.EqualTo(2));
        Assert.That(list[4], Is.EqualTo(3));
    }

    [Test]
    public void Clear_AfterAdds_ResetsLengthToZero()
    {
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Resize(0);

        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void RemoveLast_WithItems_RemovesLastElement()
    {
        list.Add(10);
        list.Add(20);
        list.Add(30);

        list.RemoveLast();

        Assert.That(list.Length, Is.EqualTo(2));
        Assert.That(list[0], Is.EqualTo(10));
        Assert.That(list[1], Is.EqualTo(20));
    }

    [Test]
    public void RemoveLast_OnEmptyList_LengthRemainsZero()
    {
        list.RemoveLast();

        Assert.That(list.Length, Is.EqualTo(0));
    }

    [Test]
    public void AsSpan_AfterAdds_ReturnsCorrectValues()
    {
        list.Add(5);
        list.Add(10);
        list.Add(15);

        Span<int> span = list.AsSpan();

        Assert.That(span.Length, Is.EqualTo(3));
        Assert.That(span[0], Is.EqualTo(5));
        Assert.That(span[1], Is.EqualTo(10));
        Assert.That(span[2], Is.EqualTo(15));
    }

    [Test]
    public void Indexer_BeyondArrayCapacity_ThrowsIndexOutOfRangeException()
    {
        var smallList = new FastList<int>(4);
        smallList.Add(1);

        Assert.That(() => _ = smallList[100], Throws.TypeOf<IndexOutOfRangeException>());
    }

    [Test]
    public void Constructor_WithZeroInitialLength_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => new FastList<int>(0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Add_WithInitialCapacityOne_GrowsCorrectly()
    {
        FastList<int> smallList = new FastList<int>(1);

        smallList.Add(1);
        smallList.Add(2);

        Assert.That(smallList.Length, Is.EqualTo(2));
        Assert.That(smallList[0], Is.EqualTo(1));
        Assert.That(smallList[1], Is.EqualTo(2));
    }
}

public class FastListStructTests
{
    FastListStruct<int> list;

    [SetUp]
    public void Setup()
    {
        list = new();
    }

    [Test]
    public void Add_MultipleItems_ValuesAccessibleViaIndexer()
    {
        list.Add(10);
        list.Add(20);
        list.Add(30);

        Assert.That(list.Length, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(10));
        Assert.That(list[1], Is.EqualTo(20));
        Assert.That(list[2], Is.EqualTo(30));
    }

    [Test]
    public void Add_BeyondInitialCapacity_GrowsCorrectly()
    {
        var smallList = new FastListStruct<int>(4);

        for (int i = 0; i < 10; i++)
        {
            smallList.Add(i * 100);
        }

        Assert.That(smallList.Length, Is.EqualTo(10));
        for (int i = 0; i < 10; i++)
        {
            Assert.That(smallList[i], Is.EqualTo(i * 100));
        }
    }

    [Test]
    public void SwapRemove_MiddleElement_SwapsLastIntoPosition()
    {
        list.Add(10);
        list.Add(20);
        list.Add(30);

        bool swapped = list.SwapRemove(0);

        Assert.That(swapped, Is.True);
        Assert.That(list.Length, Is.EqualTo(2));
        Assert.That(list[0], Is.EqualTo(30));
        Assert.That(list[1], Is.EqualTo(20));
    }

    [Test]
    public void AsReadOnlySpan_AfterAdds_ReturnsCorrectValues()
    {
        list.Add(7);
        list.Add(14);

        ReadOnlySpan<int> span = list.AsReadOnlySpan();

        Assert.That(span.Length, Is.EqualTo(2));
        Assert.That(span[0], Is.EqualTo(7));
        Assert.That(span[1], Is.EqualTo(14));
    }

    [Test]
    public void LastIndex_AfterAdds_ReturnsCorrectValue()
    {
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.LastIndex, Is.EqualTo(2));
    }
}
