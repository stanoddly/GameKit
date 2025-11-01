Prepare a skeleton for a project called `GameKit.SdlangCompileTask` in `src/`. At the moment the project won't do much (raise not implemented),
only provide a custom MSBuild task to compiler slang shader for SDL3.

It should be usable as something like this:
```
<SdlangCompile InputFile="path" />
```

Not sure if `InputFile` is the right choice, figure that out based on good practices from MSBuild.
