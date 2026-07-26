# NetArchTest.Rules

Package: `NetArchTest.Rules` 1.3.2 (implemented in
`tests/PickMeWhatToListen.ArchitectureTests/LayerDependencyTests.cs`). This
is the last release of the original package (unmaintained since 2021); the
API below is still current as of this writing (verified via Context7).

## Pattern used in this repo

```csharp
using NetArchTest.Rules;

var result = Types.InAssembly(typeof(SomeWpfType).Assembly)
    .ShouldNot()
    .HaveDependencyOn("Microsoft.EntityFrameworkCore")
    .GetResult();

Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames));
```

Rules enforced for this repo (see `ARCHITECTURE.md`):

1. `PickMeWhatToListen.Domain` types should not have a dependency on
   `PickMeWhatToListen.Application`, `.Infrastructure`, or `.Wpf`.
2. `PickMeWhatToListen.Application` types should not have a dependency on
   `PickMeWhatToListen.Infrastructure` or `.Wpf`.
3. Types in `PickMeWhatToListen.Wpf` (and anything outside `.Infrastructure`
   generally) should not have a dependency on `Microsoft.EntityFrameworkCore`.

`Types.InAssembly(...)` takes a loaded `Assembly` — the test project needs a
project reference to every assembly it inspects (already wired for
`PickMeWhatToListen.ArchitectureTests` against `Domain`, `Application`,
`Infrastructure`, and `Wpf`).

`HaveDependencyOnAny(params string[])` checks against multiple
namespaces/types in one rule (used for rules 1 and 2 above); use plain
`HaveDependencyOn(string)` for a single target (rule 3).
