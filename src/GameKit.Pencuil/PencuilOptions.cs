namespace GameKit.Pencuil;

public class PencuilOptions
{
    public int Order { get; init; } = 10_000;
    public int InputOrder { get; init; } = -10_000;
    public bool ClearTarget { get; init; }
}
