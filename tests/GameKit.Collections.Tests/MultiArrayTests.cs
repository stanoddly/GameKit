namespace GameKit.Collections.Tests;

public class MultiArrayStructTests
{
    MultiArrayStruct<int, double> _array;

    [SetUp]
    public void Setup()
    {
        _array = new();
    }

    [Test]
    public void Add_SingleItem_LengthIsOne()
    {
        _array.Add(1, 1.5);

        Assert.That(_array.Length, Is.EqualTo(1));
    }

    [Test]
    public void Add_MultipleItems_LengthIncreases()
    {
        _array.Add(10, 1.1);
        _array.Add(20, 2.2);
        _array.Add(30, 3.3);

        Assert.That(_array.Length, Is.EqualTo(3));
    }

    [Test]
    public void GetValue1_AfterAdd_ReturnsCorrectValue()
    {
        _array.Add(42, 3.14);
        _array.Add(99, 2.71);

        Assert.That(_array.GetValue1(0), Is.EqualTo(42));
        Assert.That(_array.GetValue1(1), Is.EqualTo(99));
    }

    [Test]
    public void GetValue2_AfterAdd_ReturnsCorrectValue()
    {
        _array.Add(42, 3.14);
        _array.Add(99, 2.71);

        Assert.That(_array.GetValue2(0), Is.EqualTo(3.14));
        Assert.That(_array.GetValue2(1), Is.EqualTo(2.71));
    }

    [Test]
    public void TryGet_ValidIndex_ReturnsTrueWithValues()
    {
        _array.Add(42, 3.14);

        bool result = _array.TryGet(0, out int v1, out double v2);

        // BUG: The condition is inverted (index >= _count returns true),
        // so a valid index (0 < 1) actually returns false.
        Assert.That(result, Is.True);
        Assert.That(v1, Is.EqualTo(42));
        Assert.That(v2, Is.EqualTo(3.14));
    }

    [Test]
    public void TryGet_OutOfRangeIndex_ReturnsFalse()
    {
        _array.Add(42, 3.14);

        bool result = _array.TryGet(5, out int v1, out double v2);

        Assert.That(result, Is.False);
        Assert.That(v1, Is.EqualTo(default(int)));
        Assert.That(v2, Is.EqualTo(default(double)));
    }

    [Test]
    public void TryGetButFirst_ValidIndex_ReturnsTrueWithValue2()
    {
        _array.Add(42, 3.14);

        bool result = _array.TryGetButFirst(0, out double v2);

        // BUG: Same inverted bounds as TryGet — valid index returns false.
        Assert.That(result, Is.True);
        Assert.That(v2, Is.EqualTo(3.14));
    }

    [Test]
    public void SwapRemove_MiddleElement_MovesLastToMiddle()
    {
        _array.Add(10, 1.1);
        _array.Add(20, 2.2);
        _array.Add(30, 3.3);

        _array.SwapRemove(0);

        Assert.That(_array.Length, Is.EqualTo(2));
        Assert.That(_array.GetValue1(0), Is.EqualTo(30));
        Assert.That(_array.GetValue2(0), Is.EqualTo(3.3));
    }

    [Test]
    public void SwapRemove_LastElement_DecrementsLength()
    {
        _array.Add(10, 1.1);
        _array.Add(20, 2.2);

        bool wasLast = _array.SwapRemove(1);

        Assert.That(wasLast, Is.True);
        Assert.That(_array.Length, Is.EqualTo(1));
    }

    [Test]
    public void EnsureCapacity_BeyondDefault_GrowsCorrectly()
    {
        for (int i = 0; i < 64; i++)
        {
            _array.Add(i, i * 0.5);
        }

        Assert.That(_array.Length, Is.EqualTo(64));
        Assert.That(_array.GetValue1(63), Is.EqualTo(63));
        Assert.That(_array.GetValue2(63), Is.EqualTo(31.5));
    }

    [Test]
    public void Clear_AfterAdds_ResetsLengthToZero()
    {
        _array.Add(10, 1.1);
        _array.Add(20, 2.2);
        _array.Add(30, 3.3);

        _array.Clear();

        Assert.That(_array.Length, Is.EqualTo(0));
    }
}
