# Package publication

Pixely is published as one prerelease package containing every project under `src`. The package version format is `0.0.N-alpha`.

## One-time setup

Create a nuget.org trusted-publishing policy with these GitHub details:

- repository owner: `stanoddly`
- repository: `Pixely`
- workflow file: `publish.yml`

No NuGet API key is stored in GitHub.

If nuget.org marks a new policy as pending full activation, publish within its displayed seven-day activation window. The window can be restarted from the policy page if it expires.

## Publishing

Open **Actions**, select **Publish Pixely prerelease**, select `main`, and click **Run workflow**.

The workflow rejects runs outside the default branch, selects the next `0.0.N-alpha` version, and packs the runtime assemblies and build integrations. Source and package validation are not repeated because the protected default branch requires successful checks against the current branch state before merging. Pull request checks validate the package through isolated consumers on every supported host. The publication workflow creates the version tag, publishes the NuGet and symbol packages, and creates a GitHub prerelease. `dotnet nuget push` publishes the adjacent `.snupkg` through the configured symbol source.

Pixely depends on the exact `SlangDxcBundle.Toolchain` version used to build its shader compiler. NuGet restores that dependency, and the five-host consumer matrix compiles SPIR-V, DXIL, and Metal output directly from its global-package directory.

Publication runs are serialized. GitHub retains one pending run for the publication concurrency group and replaces an older pending run when another is dispatched.

## Recovery

The workflow does not reuse a version after creating its tag. If publication fails after tag creation, start a new workflow run. The next run advances the version, so failed publications may leave gaps in the prerelease sequence.
