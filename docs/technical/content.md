# Content Loading

The `ContentManager` is a component that loads game assets from a [virtual filesystem](filesystem.md). It draws an inspiration from
XNA 4.0 and its [
`ContentManager`](https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb195436(v=xnagamestudio.40)).

## XNA inspired interface with a twist

The basic interface mirrors XNA's approach:

```csharp
public interface IContentManager
{
    public TContent Load<TContent>(string path);
    public Stream OpenStream(string path);
}
```

Unlike XNA's `ContentManager` which loads ready-to-use assets, GameKit's implementation relies on injectable
`IContentLoader<TContent>` instances. Here's how to load an image:

```csharp
ContentManager contentManager = new ContentManager(fileSystem, [new StbImageLoader()])

// Loads an image via StbImageLoader
Image image = contentManager.Load<Image>("textures/sprite.png");
```

Note: without any registered `IContentLoader<Image>`, GameKit's `ContentManager.Load` would throw a
`NotSupportedException`.

Most users should use `GameKitApp.Content` which already supports various content types out of the box. Direct
`ContentManager` initialization is primarily for advanced scenarios.

## Content Processing Flexibility

Content loaders can either return ready-to-use game assets or raw assets that require further processing. This decision
is made by the `IContentLoader<TContent>` implementation, enabling optimization strategies outside of the `ContentManager` like:

- Loading GPU-related resources in parallel
- Batch uploading resources to the GPU

## Type Constraints and Generic Variance

The `TContent` type parameter is constrained to reference types (`where TContent: class`). This design decision enables
efficient dependency injection and reasonable type safety while avoiding runtime reflection.

`IContentLoader<out TContent>` is [contravariant](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/creating-variant-generic-interfaces),
allowing implicit conversion to `IContentLoader<object>`. This enables simple dependency injection container
configuration and type recognition via `IContentLoader<TContent>.SupportedType`.

The class constraint is necessary because value types cannot be used as generic arguments for contravariant interfaces
targeting `object`, and interface types cannot be used to maintain contravariance.

An alternative approach using a non-generic base interface was considered:

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

However, this would require runtime type checking through reflection and lose compile-time type safety, without
guaranteeing proper `IContentLoader<TContent>` implementation.

## IContentLoader<TContent> Design

The `Load` method of `IContentLoader<TContent>` takes multiple parameters instead of a direct stream:

```csharp
TContent Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path);
```

While accepting a stream might seem simpler, the current design enables complex loading scenarios like:

- Loading content that depends on other resources (e.g., glTF models with external textures)
- Processing directory-based content
- Handling nested content loading through the same ContentManager

## Content Loader Interface vs Abstract Class Design

`IContentLoader` is designed as an interface rather than an abstract class for these reasons:

- Only interfaces support generic covariance (`IContentLoader<out TContent>`)
- Content loaders can implement multiple `IContentLoader<out TContent>` interfaces with different `TContent` types, allowing code reuse for similar content types

## Interface-Based Design for ContentManager

The rest of the GameKit classes depend on `IContentManager` rather than `ContentManager` directly. This enables:
- Unit testing with mock implementations
- Custom implementations for different use cases

`ContentManager` is sealed to allow .NET to optimize interface calls, effectively eliminating any interface overhead in normal usage.

## UNRESOLVED: Content unloading strategy

[#24](https://github.com/stanoddly/GameKit/issues/24)

