namespace GameKit.Collections.Tests;

public class BitSetStructTests
{
    BitSetStruct bitSet;

    [SetUp]
    public void Setup()
    {
        bitSet = new BitSetStruct(128);
    }

    [Test]
    public void Set_SingleBit_GetReturnsTrue()
    {
        bitSet.Set(10, true);

        Assert.That(bitSet.Get(10), Is.True);
    }

    [Test]
    public void Get_UnsetBit_ReturnsFalse()
    {
        Assert.That(bitSet.Get(5), Is.False);
    }

    [Test]
    public void Set_ThenClear_GetReturnsFalse()
    {
        bitSet.Set(20, true);
        bitSet.Set(20, false);

        Assert.That(bitSet.Get(20), Is.False);
    }

    [Test]
    public void Set_MultipleBits_AllReturnTrue()
    {
        bitSet.Set(0, true);
        bitSet.Set(63, true);
        bitSet.Set(64, true);
        bitSet.Set(127, true);

        Assert.That(bitSet.Get(0), Is.True);
        Assert.That(bitSet.Get(63), Is.True);
        Assert.That(bitSet.Get(64), Is.True);
        Assert.That(bitSet.Get(127), Is.True);
    }

    [Test]
    public void GetSet_OnUnsetBit_ReturnsFalseAndSetsBit()
    {
        bool previous = bitSet.GetSet(15, true);

        Assert.That(previous, Is.False);
        Assert.That(bitSet.Get(15), Is.True);
    }

    [Test]
    public void GetSet_OnSetBit_ReturnsTrueAndClearsBit()
    {
        bitSet.Set(15, true);

        bool previous = bitSet.GetSet(15, false);

        Assert.That(previous, Is.True);
        Assert.That(bitSet.Get(15), Is.False);
    }

    [Test]
    public void ClearAll_AfterSettingBits_AllBecomeUnset()
    {
        bitSet.Set(0, true);
        bitSet.Set(42, true);
        bitSet.Set(100, true);

        bitSet.ClearAll();

        Assert.That(bitSet.Get(0), Is.False);
        Assert.That(bitSet.Get(42), Is.False);
        Assert.That(bitSet.Get(100), Is.False);
    }

    [Test]
    public void Indexer_SetAndGet_BehavesLikeSetAndGet()
    {
        bitSet[33] = true;

        Assert.That(bitSet[33], Is.True);
        Assert.That(bitSet[34], Is.False);
    }
}

public class DynamicBitSetStructTests
{
    DynamicBitSetStruct bitSet;

    [SetUp]
    public void Setup()
    {
        bitSet = new DynamicBitSetStruct(64);
    }

    [Test]
    public void Set_SingleBit_GetReturnsTrue()
    {
        bitSet.Set(10, true);

        Assert.That(bitSet.Get(10), Is.True);
    }

    [Test]
    public void Set_BeyondInitialCapacity_GrowsAndGetReturnsTrue()
    {
        nuint beyondCapacity = bitSet.Capacity + 50;

        bitSet.Set(beyondCapacity, true);

        Assert.That(bitSet.Get(beyondCapacity), Is.True);
        Assert.That(bitSet.Capacity, Is.GreaterThanOrEqualTo(beyondCapacity));
    }

    [Test]
    public void Set_ThenClear_GetReturnsFalse()
    {
        bitSet.Set(20, true);
        bitSet.Set(20, false);

        Assert.That(bitSet.Get(20), Is.False);
    }

    [Test]
    public void Set_MultipleBits_AllReturnTrue()
    {
        bitSet.Set(0, true);
        bitSet.Set(31, true);
        bitSet.Set(63, true);

        Assert.That(bitSet.Get(0), Is.True);
        Assert.That(bitSet.Get(31), Is.True);
        Assert.That(bitSet.Get(63), Is.True);
        Assert.That(bitSet.Get(1), Is.False);
    }

    [Test]
    public void Get_BeyondCapacity_ReturnsFalse()
    {
        Assert.That(bitSet.Get(9999), Is.False);
    }

    [Test]
    public void GetSet_BeyondCapacity_GrowsAndReturnsFalse()
    {
        nuint beyondCapacity = bitSet.Capacity + 10;

        bool previous = bitSet.GetSet(beyondCapacity, true);

        Assert.That(previous, Is.False);
        Assert.That(bitSet.Get(beyondCapacity), Is.True);
    }

    [Test]
    public void ClearAll_AfterSettingBits_AllBecomeUnset()
    {
        bitSet.Set(5, true);
        bitSet.Set(50, true);

        bitSet.ClearAll();

        Assert.That(bitSet.Get(5), Is.False);
        Assert.That(bitSet.Get(50), Is.False);
    }
}
