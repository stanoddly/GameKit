# Package publication review checklist

This checklist tracks the actionable findings from the independent Claude Opus 5 review of the ticket #393 implementation.

## Publication correctness

- [x] Make artifact upload safe when failed jobs are rerun.
- [x] Pack once with the publication properties and use that exact archive for consumer tests and publication.
- [x] Verify the package commit after pushing so `--skip-duplicate` cannot hide a version collision.
- [x] Keep build execution out of the job that holds NuGet OIDC and repository write permissions.
- [x] Run the complete existing CI validation before publication instead of a reduced copy.
- [x] Document recovery when a published NuGet version has no corresponding repository tag.

## Package maintenance and verification

- [x] Verify declared package dependencies against the runtime source projects rather than another hardcoded dependency list.
- [x] Load every packaged runtime assembly in the isolated consumer test.
- [x] Collect runtime assemblies and symbols from MSBuild project outputs instead of hardcoded `bin/net10.0` paths.
- [x] Add the packaging project to the solution build.
- [x] Use separate package-feed and restore-cache directories in integration tests.
- [x] Exclude generated consumer `bin` and `obj` trees from test-project items.
- [x] Replace ineffective placeholder assertions with checks for NuGet `_._` entries.
- [x] Add timeouts to nested `dotnet` processes and publication/consumer jobs.

## Security and documentation

- [x] Pin publication workflow actions to immutable commit SHAs.
- [x] Verify downloaded Slang bundles against platform-specific SHA-256 digests.
- [x] Acquire Slang/DXC during packaging and embed verified platform tool directories so consumer builds have no non-NuGet download.
- [x] Pass the temporary NuGet credential through an environment variable and make symbol publication explicit.
- [x] Distinguish package-consumer shader setup from in-repository project-reference setup.
- [x] Document trusted-publishing activation and concurrency behavior.

## Reviewed and intentionally unchanged

- [x] Keep `Pixely.Architecture.Testing` in `lib/net10.0`; ticket #393 explicitly requires every runtime project.
- [x] Keep the package version derived from repository tags without a routine version input; this is an explicit ticket requirement.
- [x] No NU5100 change is required; the exact publication pack succeeds with warnings treated as errors.
- [x] No separator-specific fix is required for correctness; package-provided shader compilation succeeds on Linux. The path will still be normalized for consistency.
