# Migrating from Negative Width/Height to SpriteFlip

## What Changed

- `ShortRectangle.Width` and `Height` changed from `short` to `ushort` — negative dimensions are no longer allowed
- A new `SpriteFlip` enum (`None`, `Horizontal`, `Vertical`, `Both`) replaces the negative-width/height hack
- `SpriteAsset` and `AnimatedSpriteAsset` have a new `Flip` parameter
- `ShortRectangle.Size` now returns `UShortVector2` instead of `ShortVector2`

## JSON Format

### Before

Mirroring was encoded via negative width/height in `textureRegion`:

```json
{"texture": "hero.png", "textureRegion": [31, 0, -32, 32]}
```

### After

Dimensions are always positive. Mirroring is a separate `flip` field:

```json
{"texture": "hero.png", "textureRegion": [0, 0, 32, 32], "flip": "Horizontal"}
```

Valid `flip` values: `"None"` (default if omitted), `"Horizontal"`, `"Vertical"`, `"Both"`.

### Conversion Rules

| Old `textureRegion` | New `textureRegion` | `flip` |
|---|---|---|
| `[x, y, w, h]` (both positive) | `[x, y, w, h]` | omit or `"None"` |
| `[x, y, -w, h]` | `[x-w+1, y, w, h]` | `"Horizontal"` |
| `[x, y, w, -h]` | `[x, y-h+1, w, h]` | `"Vertical"` |
| `[x, y, -w, -h]` | `[x-w+1, y-h+1, w, h]` | `"Both"` |

For example, `[31, 0, -32, 32]` becomes `[0, 0, 32, 32]` with `"flip": "Horizontal"` (since `31 - 32 + 1 = 0`).

## Code Changes

### Constructing ShortRectangle

```diff
-new ShortRectangle(0, 0, (short)width, (short)height)
+new ShortRectangle(0, 0, (ushort)width, (ushort)height)
```

If the source is already `ushort` (e.g. from `ShortSize`), the cast is unnecessary:

```diff
-new ShortRectangle(0, 0, (short)size.Width, (short)size.Height)
+new ShortRectangle(0, 0, size.Width, size.Height)
```

### Constructing SpriteAsset

```diff
-new SpriteAsset(texture, region)
+new SpriteAsset(texture, region)                          // no flip (default)
+new SpriteAsset(texture, region, SpriteFlip.Horizontal)   // flipped
```

### Using Size

`ShortRectangle.Size` and `SpriteAsset.Size` now return `UShortVector2` (with `ushort X, Y`) instead of `ShortVector2` (with `short X, Y`). This converts implicitly to `Vector2`, so most usage is unaffected. If you need `int` values:

```diff
-ShortVector2 size = sprite.Size;
+UShortVector2 size = sprite.Size;
```

### Custom UV Calculation

If you call `Texture.CalculateTextureRegionUVs()` directly:

```diff
-Vector4 uvs = texture.CalculateTextureRegionUVs(region);
+Vector4 uvs = texture.CalculateTextureRegionUVs(region, flip);
```

The second parameter defaults to `SpriteFlip.None` so existing calls without flip still compile.
