# 0003-standalone-exe

Status: Completed
Spec: [docs/product-specs/standalone-exe.md](../../product-specs/standalone-exe.md)

## Goal

Make `PickMeWhatToListen.Wpf` publishable as a single self-contained
`win-x64` `.exe` via `dotnet publish`, and document the command.

## Scope

- In: publish-related MSBuild properties on
  `PickMeWhatToListen.Wpf.csproj` (`RuntimeIdentifier`, `SelfContained`,
  `PublishSingleFile`), `AGENTS.md` command, empirical verification that
  the published exe actually launches (given the known .NET 10
  single-file WPF regression — see spec).
- Out (see `standalone-exe.md` non-goals): framework-dependent option,
  installer/MSIX, CI release automation, non-Windows RIDs.

## Plan

- [x] Add `RuntimeIdentifier=win-x64`, `SelfContained=true`,
      `PublishSingleFile=true` to `PickMeWhatToListen.Wpf.csproj` only
      (not `Directory.Build.props` — these are publish-shape properties
      specific to the exe project, not something `Domain`/`Application`
      test projects should inherit).
- [x] Run `dotnet publish -c Release` and confirm a single
      `PickMeWhatToListen.Wpf.exe` is produced.
- [x] Empirically launch the published exe (not `dotnet run`) from a
      normal folder path and confirm the main window opens and the
      catalog loads — this is the concrete check for the known SDK
      10.0.200+ single-file BAML regression.
- [x] Document the publish command in `AGENTS.md`.
- [x] Move this plan to `completed/`, flip spec status to Implemented.

## Decisions & deviations log

- **Setting `RuntimeIdentifier`/`SelfContained`/`PublishSingleFile`
  directly on the csproj (rather than only passing them as
  `dotnet publish -r win-x64 ...` command-line args) also affects plain
  `dotnet build`/`dotnet run`/`dotnet test`** — the `Wpf` project now
  builds self-contained for `win-x64` even for local dev, and its output
  moved from `bin/Debug/net10.0-windows/` to
  `bin/Debug/net10.0-windows/win-x64/`. Verified `dotnet build` and
  `dotnet test` (all 36 tests, including `ArchitectureTests`, which
  references the `Wpf` assembly via `ProjectReference`) still pass
  unaffected — MSBuild resolves the moved output path transparently.
  Accepted this trade-off over command-line-only flags because the spec
  wants a plain `dotnet publish` (no extra flags to remember) to produce
  the standalone exe by default.
- **Empirically verified the published exe launches on the pinned SDK
  (`10.0.302`)**, despite a confirmed regression in SDK 10.0.200/10.0.202
  where `PublishSingleFile` WPF apps crash on startup with a
  `NotImplementedException` in the BAML reader
  ([dotnet/wpf#11678](https://github.com/dotnet/wpf/issues/11678)). Ran
  the published `.exe` directly (not `dotnet run`) and confirmed it
  reached the EF Core migration/query logging that only happens after the
  main window's `ListBox` binding triggers a load — i.e. XAML/BAML parsed
  fine and the window rendered. No code change needed since the bug
  doesn't reproduce on `10.0.302`, but noted the SDK version dependency in
  `standalone-exe.md` so a future SDK bump gets re-verified rather than
  assumed safe.
- Single-file publish still emits a handful of native DLLs alongside the
  `.exe` (`wpfgfx_cor3.dll`, `PresentationNative_cor3.dll`,
  `D3DCompiler_47_cor3.dll`, `PenImc_cor3.dll`, `vcruntime140_cor3.dll`,
  and SQLite's `e_sqlite3.dll`) — these are native (non-managed)
  dependencies that single-file bundling can't embed. Documented in
  `AGENTS.md`/spec that the whole `publish` folder must be copied, not
  just the `.exe`.

## Open items / follow-ups

- None open. If CI-driven release automation or an installer is wanted
  later, that's new scope — see `standalone-exe.md` non-goals.
