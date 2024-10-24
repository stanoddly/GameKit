# Content loading

The `ContentManager` is a component which loads game assets from a virtual filesystem.

It draws an inspiration from XNA 4.0 and its [`ContentManager`](https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb195436(v=xnagamestudio.40)).

Its basic interface is mostly the same as XNA's:

```csharp
public interface IContentManager
{
    public TContent Load<TContent>(string path);
    public Stream OpenStream(string path);
}
```

ContentManager is build as part of the GameKitBuilder, however it's possible to create via a constructor with these arguments:

```csharp
public ContentManager(FileSystem fileSystem, IEnumerable<IContentLoader<object>> contentLoaders)
```

This is important to highlight, because GameKit's `ContentManager.Load` only throws `NotSupportedException` without any registered `IContentLoaders<TContent>`. They are supposed to be injected via constructor to make things work.

## `TContent` is constrained to a class (`where TContent: class`)

The constrain is there to have a decent way to inject `ContentLoader` instances into a `ContentManager`. Usually, `TContent` is simply impractical to be handled as a value anyway. But let's go into technical details.

The `IContentLoader<out TContent>` is [a contravariant](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/creating-variant-generic-interfaces), so it could be implicitly casted to `IContentLoader<object>`. Therefore ContentManager can accept `IEnumerable<IContentLoader<object>>`, something that is understood by dependency injection containers. The type is recognized by `IContentLoader<TContent>.SupportedType` property and loaded by `ContentManager`.

A generic argument for a covariant interface can't be a value type if the target is `object`. Also, a generic argument cannot be an interface to make the covariancy to work.

An alternative solution would be to avoid a covariant completely and have a common interface generic-free interface `IContentLoader` with `SupportedType`, which is implemented by `IContentLoader<TContent>`:

```csharp
public interface IContentLoader
{
    Type SupportedType { get; }
}

public interface IContentLoader<TContent>: IContentLoader
{
    Type IContentLoader.SupportedType => typeof(TContent);
    TContent Load(IContentManager contentManager, FileSystem fileSystem, string path);
}
```

However that would mean that `IContentManager` would accept `IContentLoader` without any guarantees that such interface derives from `IContentLoader<TContent>`. The only way how to check this for a generic interface would lead to a type reflection during execution (see [here](https://stackoverflow.com/a/18233467)). So a covariant `IContentLoader<out TContent>` provides strong guarantees.

So in the end `TContent` is indeed constrained to a class.

## IContentLoader<TContent>.Load doesn't accept `stream` directly

A method to load specific content `IContentLoader<out TContent>.Load` doesn't accept a stream directly, but plenty of arguments instead:

```csharp
TContent Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path);
```

While accepting stream would be more straighfoward, there might be cases where one content type requires to load several other content types, or where a path is intentionally a directory. A good example is [glTF](https://en.wikipedia.org/wiki/GlTF) which can have embedded textures but also external textures. External textures could be handled by `ContentManager`, so that's why it's passed alongside `FileSystem` and `path`.

## `IContentLoader` isn't an abstract class, but an interface

Besides the fact that only interfaces could be covariants, content loaders may implement multiple `IContentLoader<out TContent>` types with different `TContent`, since certain contents may be quite similar in certain aspects.

## Other classes expect `IContentManager`, not `ContentManager` directly

The reason is for unit testing but also to provide a choice to ignore `ContentManager` completely and implement a new class that supports different use cases.

It's not a big deal anyway - `ContentManager` is intentionally a sealed class, so under normal circumstances modern versions of .NET will optimize the interface and there won't be any interface behind the scenes.
