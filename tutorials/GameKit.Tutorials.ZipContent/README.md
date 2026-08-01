# ZIP content

This tutorial compiles a Slang shader and uses two distributions for the same virtual content path:

- `dotnet build` copies `Content` beside the executable.
- `dotnet publish` creates `Content.pk3` beside the executable.

Run the loose-directory build from the repository root:

```bash
dotnet run --project tutorials/GameKit.Tutorials.ZipContent
```

Publish and run the ZIP distribution:

```bash
dotnet publish tutorials/GameKit.Tutorials.ZipContent -o /tmp/gamekit-zip-content
dotnet /tmp/gamekit-zip-content/GameKit.Tutorials.ZipContent.dll
```

`AddContentFromZipPattern` and `AddContentFromDirectoryPattern` both resolve beside the application. The directory source is registered last, so it overrides the archive when both contain the same virtual path.

The package target reads directly from the source `Content` tree. It does not first publish a loose directory and does not need to remove one. This packages every file in that tree, including the Slang source. Use a separate staging directory when a release must exclude source files.
