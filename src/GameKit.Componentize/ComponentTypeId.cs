namespace GameKit.Componentize;

internal static class ComponentTypeId
{
    public static int NextId = 0;
}

public static class ComponentTypeId<T> where T: GameComponent
{
    public static readonly int Id;
    public static readonly string Name;

    static ComponentTypeId()
    {
        Id = ++ComponentTypeId.NextId;

        Name = typeof(T).Name;
    }
}
