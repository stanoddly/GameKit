# WIP: Slang NuGet Review

- [x] Centralize the Slang version, DXC version, extraction allowlists, and required-file expectations so a version update cannot drift between packaging, source builds, and tests; verify downloads with the release's `.sha256` sidecars instead of copying their digests into Pixely.
- [x] Replace a corrupt or truncated cached Slang archive automatically instead of failing every subsequent build until it is deleted manually.
- [x] Keep packaging extraction non-incremental because publication runs infrequently on fresh CI runners; removing each staging directory also prevents stale allowlist entries.
- [x] Invoke packaged `slangc` directly from NuGet's global package folder instead of copying it and running `chmod` during consumer builds.
- [x] Do not construct `SlangToolSourceDirectory` when the build host has no supported `SlangPlatform` mapping.
- [x] Validate immediately after filtered extraction that every required compiler, library, manifest, and license file exists.
- [x] Remove the redundant `DownloadSlang` to `ValidateSlangArchive` target edge.
- [x] Replace opaque `Single()` failures with an exact comparison against the canonical required-file manifest.
- [x] Name the package-size limit and document it as the nuget.org service limit.
- [x] Make the cross-RID package test assert generated shader outputs in addition to the selected host directory.
- [x] Keep the package-level `MIT` license metadata and add a root third-party notice directing consumers to the complete embedded Slang and DXC license sets.
- [x] Keep the five-host package-consumer CI matrix as the required validation for the trimmed platform directories.
