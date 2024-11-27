namespace GameKit.Collections.Tests;

internal readonly struct TestKey: IKey<TestKey>
{
    public TestKey(int index, int version)
    {
        Index = index;
        Version = version;
    }

    public int Index { get; }
    public int Version { get; }

    public static TestKey TombStone { get; } = new TestKey(int.MaxValue, int.MaxValue);
    public bool IsTombStone()
    {
        return Index == int.MaxValue;
    }

    public TestKey WithIndex(int index)
    {
        return new TestKey(index, Version);
    }

    public TestKey WithVersion(int version)
    {
        return new TestKey(Index, version);
    }
}