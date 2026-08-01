# PickMeWhatToListen — agent map

One-liner: WPF desktop app that catalogues artists you want to listen to, and picks a random not-yet-picked one on demand.

Stack: .NET 10, WPF, EF Core + SQLite, CommunityToolkit.Mvvm, xUnit, NetArchTest.

This file is a table of contents, not an encyclopedia. If a rule or fact isn't
here, it lives in one of the places below — look there before asking or
guessing.

## Where to look next

- Layer rules & dependency diagram: [ARCHITECTURE.md](ARCHITECTURE.md)
- Operating principles for this repo: [docs/design-docs/core-beliefs.md](docs/design-docs/core-beliefs.md)
- What to build / product scope: [docs/product-specs/index.md](docs/product-specs/index.md)
- What's currently in flight: [docs/exec-plans/active/](docs/exec-plans/active/)
- Known tech debt: [docs/exec-plans/tech-debt-tracker.md](docs/exec-plans/tech-debt-tracker.md)
- Mechanical/style rules: `.cursor/rules/` (auto-loaded by Cursor per file glob — see the files themselves for what each covers)
- Vendor doc snapshots for third-party packages: [docs/references/](docs/references/)
- Generated artifacts (e.g. current DB schema): [docs/generated/](docs/generated/)

## Run / build

```
dotnet build
dotnet test
dotnet run --project src/PickMeWhatToListen.Wpf
```

To produce a standalone `.exe` that runs without a .NET SDK installed (see
[docs/product-specs/standalone-exe.md](docs/product-specs/standalone-exe.md)):

```
dotnet publish src/PickMeWhatToListen.Wpf -c Release
```

The self-contained, single-file `win-x64` exe lands in
`src/PickMeWhatToListen.Wpf/bin/Release/net10.0-windows/win-x64/publish/`
alongside a handful of native WPF/SQLite DLLs that can't be bundled into
the single file — copy the whole `publish` folder, not just the `.exe`.

EF Core migrations live in `src/PickMeWhatToListen.Infrastructure/Migrations`.
To add one after changing the model:

```
cd src/PickMeWhatToListen.Infrastructure
dotnet ef migrations add <Name> --output-dir Migrations
```

The SQLite catalog file lives at `%AppData%\PickMeWhatToListen\catalog.db` and
is created/migrated automatically on app startup — never commit it.

To wipe your local catalog (no in-app "clear all" action exists yet), close
the app and delete that file — it's recreated via migrations next launch:

```
Remove-Item "$env:AppData\PickMeWhatToListen\catalog.db"
```

## Debugging the UI

Run the app in Debug and an MCP client with the `xamlmcp` server (registered
in `.cursor/mcp.json`) can attach and inspect/drive the live visual tree —
see [docs/references/xamlmcp-wpf.md](docs/references/xamlmcp-wpf.md) for the
attach wiring and gotchas (Debug-only, one instance/session at a time,
node ids are single-snapshot).

## Workflow for any non-trivial change

1. Add/update a spec in `docs/product-specs/`.
2. Create an exec plan in `docs/exec-plans/active/` (copy `_template.md`).
3. Implement respecting the layer boundaries in `ARCHITECTURE.md`.
4. Extend tests, including `PickMeWhatToListen.ArchitectureTests` if boundaries changed.
5. Move the exec plan to `docs/exec-plans/completed/` once merged, and update `docs/generated/db-schema.md` if the model changed.

## Golden rules

- Dependency direction is one-way: `Domain <- Application <- Infrastructure/Wpf`. Nothing outside `Infrastructure` may reference `Microsoft.EntityFrameworkCore.*`.
- `Artist.IsPicked` is a **persistent** listened/not-listened marker, not a single "currently selected" flag. Don't reintroduce a single-selection concept without updating `docs/product-specs/core-loop.md` first.
- New third-party dependency? Look up its docs via Context7 first, and if the usage pattern isn't obvious, snapshot the finding under `docs/references/`.
- No business logic in WPF code-behind or XAML — it belongs in a ViewModel or `Application` service.
- In the `PickMeWhatToListen.Wpf` project, always fully-qualify `System.Windows.Application` — the sibling `PickMeWhatToListen.Application` namespace shadows the unqualified name (see `.cursor/rules/mvvm-wpf.mdc`).
