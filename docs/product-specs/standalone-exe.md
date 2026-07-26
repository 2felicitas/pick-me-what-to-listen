# Standalone executable distribution

Status: Implemented (see `docs/exec-plans/completed/0003-standalone-exe.md`)

## Goal

Let the app be launched by double-clicking an `.exe`, without a dev
environment (.NET SDK) installed on the target machine.

## Behavior

- `dotnet publish` produces a single `PickMeWhatToListen.Wpf.exe` for
  `win-x64` that bundles the .NET runtime (self-contained) — no separate
  runtime install needed on the target machine.
- Publishing is a manual, on-demand command (see `AGENTS.md`), not wired
  into CI. There's no release/tagging workflow yet — if that's wanted
  later, it's a separate change (see `tech-debt-tracker.md` if deferred).
- The published exe reads/writes the same `%AppData%\PickMeWhatToListen\catalog.db`
  as `dotnet run` — publishing doesn't change where data lives.

## Non-goals (for this spec)

- Framework-dependent publish (smaller output, requires the target machine
  to already have the .NET 10 desktop runtime) — self-contained was chosen
  instead so the app runs on a clean machine.
- An installer (MSI/MSIX) or auto-update mechanism — just a runnable
  `.exe`.
- CI-driven release automation (building/publishing on tag push) — out of
  scope until there's an actual need to distribute to someone other than
  the developer's own machine.
- Cross-platform publish (linux-x64, osx-x64) — this is a Windows-only WPF
  app.

## Known gotchas

- .NET 10 SDK versions 10.0.200/10.0.202 have a confirmed regression where
  `PublishSingleFile` WPF apps crash on startup with a
  `NotImplementedException` in the BAML reader (see
  [dotnet/wpf#11678](https://github.com/dotnet/wpf/issues/11678)). This
  repo pins SDK `10.0.302` via `global.json` — verified empirically (see
  exec plan decisions log) that the published exe actually launches on
  this SDK version before relying on single-file publish here.
