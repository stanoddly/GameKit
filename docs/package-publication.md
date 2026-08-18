# Package publication

Pixely is published as one prerelease package containing every project under `src`. The package version format is `0.0.N-alpha`.

## One-time setup

Create a nuget.org trusted-publishing policy with these GitHub details:

- repository owner: `stanoddly`
- repository: `Pixely`
- workflow file: `publish.yml`

Set the GitHub repository variable `NUGET_USER` to the nuget.org profile name that owns the trusted-publishing policy. No NuGet API key is stored in GitHub.

If nuget.org marks a new policy as pending full activation, publish within its displayed seven-day activation window. The window can be restarted from the policy page if it expires.

## Publishing

Open **Actions**, select **Publish Pixely prerelease**, select `main`, and click **Run workflow**.

The workflow selects the next `0.0.N-alpha` version, runs the complete test workflow, downloads and verifies the five pinned `slang-dxc-bundle` distributions, extracts the required tool files from each distribution into platform directories embedded in `Pixely`, and packs one archive. It validates that exact package through isolated consumers on every supported host, creates the version tag, publishes the NuGet and symbol packages, verifies the published source commit, and creates a GitHub prerelease. `dotnet nuget push` publishes the adjacent `.snupkg` through the configured symbol source.

The package job retains only `slangc`, the Slang compiler and glslang libraries, `dxcompiler`, manifests, and license material from each complete distribution. A root `THIRD-PARTY-NOTICES.md` directs consumers to the complete license set retained under each platform directory. Package integration tests enforce that layout and the nuget.org size limit. The five-host consumer matrix compiles SPIR-V, DXIL, and Metal output from the embedded directory selected by the actual build host.

Publication runs are serialized. GitHub retains one pending run for the publication concurrency group and replaces an older pending run when another is dispatched. Rerunning failed jobs for a partially completed publication reuses the same version and overwrites only that run's validation artifact.

## Recovery

NuGet package versions cannot be overwritten or reused. If a version exists on nuget.org without its corresponding repository tag, the workflow stops when the package's recorded commit differs from the selected commit. Verify the `repository commit` value in the downloaded package, recreate the missing `v0.0.N-alpha` tag at exactly that commit, push the tag, and rerun the workflow from `main`. Never point the missing tag at a different commit or reuse the published version.
