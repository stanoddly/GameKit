namespace Pixely;

/// <summary>
/// Represents an entity that can be ordered.
/// </summary>
public interface IOrderable
{
    /// <summary>
    /// The order of the entity. Lower numbers are processed first.
    /// </summary>
    int Order => 0;
}
