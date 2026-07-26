# 0001-bootstrap-repo-and-core-loop

Status: Completed
Spec: [docs/product-specs/core-loop.md](../../product-specs/core-loop.md)

## Goal

Bootstrap the repository from empty: .NET 10 / WPF solution skeleton with a
strict layered architecture, the core add-artist/pick-random loop working
end-to-end against a real SQLite database, and the harness-engineering repo
scaffolding (`AGENTS.md`, `ARCHITECTURE.md`, `docs/`, `.cursor/rules/`).

## Scope

- In: solution/project skeleton, `Artist` domain model, `ArtistCatalogService`,
  EF Core + SQLite persistence, WPF UI, this documentation structure.
- Out (see `docs/product-specs/future-ideas.md`): discography, release
  tracking, tagging. Not modeled, not referenced from the schema.

## Plan

- [x] Solution + 4 project skeletons (Domain/Application/Infrastructure/Wpf),
      `global.json`, `Directory.Build.props`, `.editorconfig`.
- [x] `Artist` entity, `IArtistRepository`, `ArtistCatalogService` + unit tests.
- [x] `AppDbContext`, `EfArtistRepository`, initial EF Core migration, AppData
      SQLite file wiring.
- [x] `MainWindow`/`MainViewModel` (CommunityToolkit.Mvvm) + Generic Host
      composition root in `App.xaml.cs`; add-artist / pick-random UI.
- [x] xUnit `PickMeWhatToListen.ArchitectureTests` project with NetArchTest
      rules enforcing the layer + EF-leakage boundaries described in
      `ARCHITECTURE.md`.
- [x] `AGENTS.md`, `ARCHITECTURE.md`, `docs/` tree, `.cursor/rules/*.mdc` (this pass).
- [x] `.github/workflows/ci.yml` (windows-latest: restore/build/test/format check).

> Note: architecture tests and CI were intentionally deferred mid-session at
> the user's request, to prioritize getting the harness-engineering docs
> written while the core loop context was fresh. Both are now done — see
> the decisions log below for how each was closed out.

## Decisions & deviations log

- **`SQLitePCLRaw.bundle_e_sqlite3` pinned to 3.0.4.** `Microsoft.EntityFrameworkCore.Sqlite`
  10.0.10 pulls in `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 transitively, which has
  a known high-severity advisory (`GHSA-2m69-gcr7-jv3q`, NU1903 warning).
  Added an explicit `PackageReference` override in
  `PickMeWhatToListen.Infrastructure.csproj`.
- **`System.Windows.Application` must be fully-qualified in `Wpf`.** Naming
  the Application layer project `PickMeWhatToListen.Application` (as
  specified) causes `CS0118` in `App.xaml.cs`: C# namespace-member lookup
  finds the sibling `PickMeWhatToListen.Application` namespace before it
  considers the `using System.Windows;` directive, so unqualified
  `Application` resolves to the namespace, not the WPF base class. Kept the
  approved project name and fixed the call site instead of renaming the
  project; documented as a standing gotcha in `.cursor/rules/mvvm-wpf.mdc`
  and `ARCHITECTURE.md` rather than a one-off fix, since it'll bite again
  anywhere `Application.Current` is used.
- **`InvariantGlobalization=false` override in `PickMeWhatToListen.Wpf.csproj`.**
  `Directory.Build.props` sets `InvariantGlobalization=true` repo-wide. WPF's
  data-binding engine calls `XmlLanguage.GetSpecificCulture()` on first
  window show and crashes (`InvalidOperationException: Cannot find
  non-neutral culture related to 'en-us'`) under invariant globalization.
  Confirmed by running the built exe directly. Overridden per-project rather
  than removing the repo-wide default, since Domain/Application/Infrastructure/tests
  have no globalization needs.
- **Timestamps stored as UTC ticks (`long`), not `DateTimeOffset`, in SQLite.**
  `Microsoft.EntityFrameworkCore.Sqlite` throws
  `NotSupportedException: SQLite does not support expressions of type
  'DateTimeOffset' in ORDER BY clauses` the first time a query orders by
  `CreatedAtUtc` — this only surfaces at query execution time, not at
  migration/build time, so it wasn't caught until the first real run of the
  app. Fixed with an EF Core value conversion
  (`HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero))`)
  in `ArtistConfiguration`, keeping `DateTimeOffset` as the domain-facing
  type. Regenerated the `InitialCreate` migration after the fix (no real
  user data existed yet).
- **`IDbContextFactory<AppDbContext>` instead of an injected `AppDbContext`.**
  WPF has no natural per-operation DI scope the way ASP.NET Core requests
  do, and `DbContext` isn't thread-safe to hold as a singleton. Used
  `AddDbContextFactory` + `CreateDbContextAsync()` per repository call
  instead (confirmed as the documented pattern for this scenario via the
  EF Core docs, not assumed from memory).
- **`ArtistPickResult` instead of throwing on an empty pool.** "No unpicked
  artists left" is an expected UI state (shown as a message), not an error
  condition, so `PickRandomAsync` returns a result object rather than
  throwing.
- **Fixed pre-existing `dotnet format --verify-no-changes` failures before
  adding CI.** Before writing `ci.yml`, ran the exact check CI will run and
  found it already failing on code untouched by this session: `App.xaml.cs`,
  `MainWindow.xaml.cs`, and the `InitialCreate` migration had a UTF-8 BOM
  (VS/`dotnet ef` scaffolding default) vs. the repo's `charset = utf-8` (no
  BOM); `AssemblyInfo.cs` had template whitespace that didn't match
  `.editorconfig`; and `AppDataDatabasePathProvider`'s `const` fields
  tripped `IDE1006` because the private-fields naming rule has no `const`
  carve-out. None of this was caught before because `dotnet build` doesn't
  run the whitespace/charset formatter, and there was no CI to run
  `dotnet format` until now. Fixed with `dotnet format whitespace` (BOM +
  whitespace) and a new `constant_fields_pascal_case` naming rule in
  `.editorconfig`; documented as a standing gotcha in
  `.cursor/rules/csharp-style.mdc` so future scaffolded files don't
  reintroduce it silently.
- **`PickMeWhatToListen.ArchitectureTests` implements exactly the 3 rules
  already planned in `docs/references/netarchtest.md`** (Domain -> no
  outer-layer dependency, Application -> no Infrastructure/Wpf dependency,
  Wpf -> no `Microsoft.EntityFrameworkCore` dependency) via
  `Types.InAssembly(...).ShouldNot().HaveDependencyOnAny(...)`/
  `HaveDependencyOn(...)`. Verified the rules actually catch violations (not
  just trivially green) by temporarily pointing the Wpf/EF-Core rule at
  `PickMeWhatToListen.Infrastructure` instead — confirmed it fails on the
  real (allowed) `Wpf -> Infrastructure` dependency — before reverting to
  the real assertion.
- **`.github/workflows/ci.yml`: single `windows-latest` job, no matrix.**
  WPF only builds on Windows, so there's nothing to matrix against. Uses
  `actions/checkout@v7` and `actions/setup-dotnet@v6` (current major
  versions as of this pass) with `global-json-file: global.json` so CI and
  local dev always resolve the same pinned SDK (`10.0.302`,
  `rollForward: latestFeature`). Steps mirror `AGENTS.md`'s documented local
  workflow (`restore` -> `build --configuration Release` ->
  `test --configuration Release` -> `format --verify-no-changes`) — ran the
  exact same four commands locally before committing to confirm the recipe
  actually passes end-to-end, since GitHub's Windows runner can't be
  dry-run locally the way `act` does for Linux jobs.

## Open items / follow-ups

None — this plan is closed out. See `docs/exec-plans/tech-debt-tracker.md`
for ongoing/deferred concerns unrelated to bootstrap scope (e.g. duplicate
artist names).
