using System.Collections.Frozen;

namespace GameKit.Content;

public interface IContentLoader<out TContent>
{
    Type SupportedType => typeof(TContent);
    TContent Load(string path);
}
