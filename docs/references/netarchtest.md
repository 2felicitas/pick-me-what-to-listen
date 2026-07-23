# NetArchTest.Rules

Package: `NetArchTest.Rules` (referenced in `PickMeWhatToListen.ArchitectureTests`,
rules not yet written — see `docs/exec-plans/tech-debt-tracker.md`).

## Pattern to use once the rules are written

```csharp
using NetArchTest.Rules;

var result = Types.InAssembly(typeof(SomeWpfType).Assembly)
    .ShouldNot()
    .HaveDependencyOn("Microsoft.EntityFrameworkCore")
    .GetResult();

Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames));
```

Planned rules for this repo (see `ARCHITECTURE.md`):

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
