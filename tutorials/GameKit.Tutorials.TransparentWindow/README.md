# Transparent Window — Patched SDL3 Guide

This tutorial requires a patched SDL3 build. SDL3's GPU API blocks transparent
windows because D3D12 doesn't support transparent swapchains. The Vulkan backend
has full support, but the check is enforced on all backends for API consistency.

Upstream issue: https://github.com/libsdl-org/SDL/issues/12410

## Prerequisites

### Fedora (atomic or classic)

```bash
sudo dnf install gcc git cmake ninja-build \
  wayland-devel wayland-protocols-devel libxkbcommon-devel libdecor-devel \
  libdrm-devel mesa-libgbm-devel vulkan-devel \
  libX11-devel libXext-devel libXrandr-devel libXcursor-devel \
  libXfixes-devel libXi-devel \
  alsa-lib-devel pulseaudio-libs-devel pipewire-devel \
  systemd-devel dbus-devel mesa-libEGL-devel
```

On atomic distros (Silverblue, Kinoite, etc.) use `rpm-ostree install --idempotent`
instead of `dnf install`. A reboot is required after layering packages.

### Ubuntu / Debian

```bash
sudo apt install build-essential git cmake ninja-build \
  libwayland-dev wayland-protocols libxkbcommon-dev libdecor-0-dev \
  libdrm-dev libgbm-dev libvulkan-dev \
  libx11-dev libxext-dev libxrandr-dev libxcursor-dev \
  libxfixes-dev libxi-dev \
  libasound2-dev libpulse-dev libpipewire-0.3-dev \
  libsystemd-dev libdbus-1-dev libegl1-mesa-dev
```

## Clone and patch SDL3

```bash
git clone https://github.com/libsdl-org/SDL.git
cd SDL
```

Open `src/gpu/SDL_gpu.c` and find the `SDL_ClaimWindowForGPUDevice` function.
Remove the transparent window guard:

```c
// Delete these lines:
if ((window->flags & SDL_WINDOW_TRANSPARENT) != 0) {
    return SDL_SetError("The GPU API doesn't support transparent windows");
}
```

## Build SDL3

```bash
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build -j$(nproc)
```

The built library will be at `build/libSDL3.so` (Linux) or the platform
equivalent.

Verify Wayland support is enabled:

```bash
grep SDL_WAYLAND build/CMakeCache.txt
# Should show: SDL_WAYLAND:BOOL=ON
```

## Using the patched library with a .NET project

The `ppy.SDL3-CS` NuGet package bundles its own native SDL3 library under
`runtimes/<rid>/native/` in the build output. To use the patched build, replace
that file after each `dotnet build`.

### Find the bundled library

After building the project, the NuGet-provided library is at:

```
bin/Debug/net10.0/runtimes/linux-x64/native/libSDL3.so
```

The exact RID directory depends on your platform (`linux-x64`, `linux-arm64`,
`osx-arm64`, `win-x64`, etc.).

### Replace it

```bash
# Build the project first
dotnet build

# Overwrite the NuGet library with the patched one
cp /path/to/SDL/build/libSDL3.so.0.5.0 \
   bin/Debug/net10.0/runtimes/linux-x64/native/libSDL3.so

# Run without rebuilding (rebuild would restore the NuGet version)
dotnet run --no-build
```

The `--no-build` flag is important — a regular `dotnet run` or `dotnet build`
will overwrite your patched library with the NuGet version.

### Alternative: LD_LIBRARY_PATH

You can also prepend the patched library directory to the library search path,
though this may not work with all .NET native library resolution strategies:

```bash
LD_LIBRARY_PATH=/path/to/SDL/build:$LD_LIBRARY_PATH dotnet run
```

### Verifying the patched library is loaded

The SDL3 log output includes a revision string. A patched build from source
will show a git hash like `SDL-3.5.0-e7b238a6e`, while the NuGet-bundled
version will show a different revision. Check the first few lines of output
for `SDL revision:`.
