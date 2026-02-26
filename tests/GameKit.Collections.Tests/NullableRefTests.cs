namespace GameKit.Collections.Tests;

public class NullableRefTests
{
    [Test]
    public void TryGet_WithValue_ExistsIsTrue()
    {
        int value = 42;
        var nullable = new NullableRef<int>(ref value);

        nullable.TryGet(out bool exists);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void Value_WithValue_ReturnsCorrectValue()
    {
        int value = 42;
        var nullable = new NullableRef<int>(ref value);

        Assert.That(nullable.Value, Is.EqualTo(42));
    }

    [Test]
    public void TryGet_Null_ExistsIsFalse()
    {
        var nullable = NullableRef<int>.Null;

        nullable.TryGet(out bool exists);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void TryGet_Null_ReturnsDefault()
    {
        var nullable = NullableRef<int>.Null;

        ref int result = ref nullable.TryGet(out _);

        Assert.That(result, Is.EqualTo(default(int)));
    }

    [Test]
    public void TrySetIfDifferent_WithDifferentValue_UpdatesAndReturnsTrue()
    {
        int value = 10;
        var nullable = new NullableRef<int>(ref value);

        bool result = nullable.TrySetIfDifferent(20);

        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo(20));
    }

    [Test]
    public void TrySetIfDifferent_WithSameValue_ReturnsFalse()
    {
        int value = 10;
        var nullable = new NullableRef<int>(ref value);

        bool result = nullable.TrySetIfDifferent(10);

        Assert.That(result, Is.False);
        Assert.That(value, Is.EqualTo(10));
    }

    [Test]
    public void TrySetIfDifferent_OnNull_ReturnsFalse()
    {
        var nullable = NullableRef<int>.Null;

        bool result = nullable.TrySetIfDifferent(42);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TrySetIfExists_WithValue_SetsAndReturnsTrue()
    {
        int value = 10;
        var nullable = new NullableRef<int>(ref value);

        bool result = nullable.TrySetIfExists(99);

        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo(99));
    }

    [Test]
    public void TrySetIfExists_OnNull_ReturnsFalse()
    {
        var nullable = NullableRef<int>.Null;

        bool result = nullable.TrySetIfExists(99);

        Assert.That(result, Is.False);
    }
}