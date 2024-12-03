namespace GameKit.Collections;

public interface IKey<TSelf> where TSelf : IKey<TSelf>
{
    int Index { get; }
    int Version { get; }
    static abstract TSelf TombStone { get; }
    bool IsTombStone();
    TSelf WithIndex(int index);
    TSelf WithVersion(int version);
    TSelf IncrementVersion();
}
