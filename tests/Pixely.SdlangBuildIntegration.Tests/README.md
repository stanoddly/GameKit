# Pixely.SdlangBuildIntegration.Tests

This project verifies clean build, publish, embedded-resource, and ZIP behavior by launching nested `dotnet` processes. Its tests are explicit so a normal `dotnet test Pixely.slnx` cannot run them concurrently with projects that have the shader MSBuild task loaded.

Shut down persistent build servers and select the integration-test category deliberately:

```shell
dotnet build-server shutdown
dotnet test tests/Pixely.SdlangBuildIntegration.Tests/Pixely.SdlangBuildIntegration.Tests.csproj --filter 'TestCategory=BuildIntegration'
```

The fixture is non-parallelizable, so its build and publish scenarios execute sequentially.
