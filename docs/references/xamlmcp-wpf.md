# XamlMcp.Wpf

Package: [`XamlMcp.Wpf`](https://github.com/trrahul/XamlMcp) `1.0.0-preview.2`
(in-process visual-tree inspection agent for `PickMeWhatToListen.Wpf`). Not in
Context7 as of this writing — this snapshot is from the upstream GitHub
README instead.

Lets an MCP client (the `xamlmcp` server registered in
[`.cursor/mcp.json`](../../.cursor/mcp.json)) attach to the running app and
walk/query/drive its visual tree — useful for an agent to debug layout,
bindings, or behavior without a human describing the screen.

## How it's wired here

`App.OnStartup` calls `this.AttachXamlMcp()`:

```csharp
protected override async void OnStartup(StartupEventArgs e)
{
#if DEBUG
    this.AttachXamlMcp();
#endif
    base.OnStartup(e);
    // ...
}
```

`AttachXamlMcp()` is itself `[Conditional("DEBUG")]` — the call disappears
from Release builds regardless. The `#if DEBUG` around it (and around
`using XamlMcp.Wpf;`) is *not* redundant, though: the `XamlMcp.Wpf`
`PackageReference` in `PickMeWhatToListen.Wpf.csproj` is scoped to
`Condition="'$(Configuration)' == 'Debug'"` so the package (and its
transitive `XamlMcp.Protocol`/`XamlMcp.Agent.Hosting` deps) never lands in
the self-contained single-file Release publish
(`docs/product-specs/standalone-exe.md`). Without the `#if DEBUG`, a
`dotnet build -c Release` would fail with `CS0246` — `[Conditional]` only
strips the call's IL emission when the *consuming* build has no `DEBUG`
symbol, it does not skip type resolution for a package that isn't even
referenced in that configuration.

## Using it

1. Run the app in Debug: `dotnet run --project src/PickMeWhatToListen.Wpf`
   (defaults to Debug) or `dotnet build/run -c Debug` explicitly.
2. In an MCP client: `list-apps` → `attach(instanceId)` → `tree` / `search` /
   `props` / `screenshot` / `input` / `action` etc.

## Gotchas

- **One running instance at a time locks the build output.** `dotnet build`/
  `dotnet run` fails to overwrite `PickMeWhatToListen.Wpf.exe` while a
  previous instance is still running — stop it first (close the window, or
  `Stop-Process` the pid from `list-apps`) before rebuilding.
- **A code change requires a full stop + rebuild + relaunch.** Attachment
  happens once in `OnStartup`; there's no hot-reload of the agent itself.
- **Node ids are single-snapshot.** Every `tree`/`search` call (and any
  mutating call with a non-`"none"` `snapshot`) mints a new `snapshotId` and
  invalidates previously issued node ids — re-query instead of reusing ids
  across calls.
- **Only one authenticated MCP session at a time**; a second client attach
  attempt queues at the OS until the first disconnects.
- **Native dialogs are invisible by default.** The "Импорт из файла..."
  button opens a Win32 `OpenFileDialog`, which `tree`/`input` can't see or
  drive — that requires the opt-in Driver (`dialog-wait`/`dialog-act`),
  enabled by adding `--enable-driver` to the `xamlmcp` server's `args` in
  `.cursor/mcp.json` (off by default; it validates file paths against
  `--driver-file-roots`, defaulting to the user profile).
- WPF-specific capability gaps (from `attach`'s reported flags): no
  style-class/pseudo-class search or mutation, no raw input (only
  `routed`), `styles` is a degraded read (declared setters/triggers, not a
  full applied cascade).
