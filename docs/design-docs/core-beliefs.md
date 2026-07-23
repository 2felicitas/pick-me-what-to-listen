# Core beliefs

This project is also a testbed for the ["harness engineering"](https://openai.com/index/harness-engineering/)
approach to working with coding agents: the repository itself — not chat
history, not a person's memory — is the system of record. These beliefs
exist to keep it that way as the app grows past its current MVP core.

1. **If it isn't in the repo, it doesn't exist for the next agent run.**
   A decision made only in a chat conversation is invisible three weeks
   later. Architectural decisions, gotchas, and conventions belong in
   `ARCHITECTURE.md`, `.cursor/rules/`, or `docs/`, not just in a PR
   description or someone's head.

2. **`AGENTS.md` is a map, not an encyclopedia.** It should stay short and
   point elsewhere. If you're tempted to add more than a couple of lines
   about a specific concern to `AGENTS.md`, it probably belongs in a
   focused `.cursor/rules/*.mdc` file or a `docs/` page instead.

3. **Boring, mechanically-checkable beats clever.** Prefer first-party /
   widely-documented libraries (EF Core, CommunityToolkit.Mvvm) over
   hand-rolled alternatives, and prefer an enforced rule (NetArchTest,
   `.editorconfig` as build errors) over a documented convention that
   nobody re-checks. Undocumented conventions rot; enforced ones don't.

4. **Spec first, then plan, then code**, for anything beyond a trivial fix.
   See `.cursor/rules/docs-and-planning.mdc` for the concrete workflow.
   This isn't process for its own sake — it's what keeps a small core
   (add artist, pick random) extensible into discography/tags/release
   tracking without accumulating undocumented coupling.

5. **Third-party library usage is verified, not assumed.** Look up current
   docs (via Context7) before using an API you "already know" — library
   surfaces change, and training data goes stale. If a library has a
   non-obvious limitation (e.g. SQLite's `DateTimeOffset` `ORDER BY`
   restriction), write it down where the next run will actually see it.

6. **Small, focused docs over one giant file.** Six 30-line `.cursor/rules/`
   files that each cover one concern are more useful to an agent than one
   500-line rulebook — see the "one big AGENTS.md" failure mode described
   in the harness engineering post.
