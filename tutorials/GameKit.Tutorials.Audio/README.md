# Audio Tutorial

Uses `GameKit.Audio` to load an Ogg buffer and play it through several reusable `AudioSource` instances.
Short sounds can be loaded with `IAudioSystem.LoadBuffer()` and assigned to `AudioSource.Clip`; longer sounds can be streamed from the virtual file system with `IAudioSystem.OpenStream()`.

## Asset

`Content/audio/beep-example.ogg` is `Beep example.ogg` from Wikimedia Commons:

https://commons.wikimedia.org/wiki/File:Beep_example.ogg

Author: D V S

License: public domain
