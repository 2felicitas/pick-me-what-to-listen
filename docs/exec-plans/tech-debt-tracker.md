# Tech debt tracker

Known, deliberately-deferred issues. Each entry should say what the debt is,
why it was accepted, and what would trigger fixing it.

## Architecture boundaries are documented but not mechanically enforced

`PickMeWhatToListen.ArchitectureTests` exists as an empty xUnit project scaffold
only; the NetArchTest rules described in `ARCHITECTURE.md` haven't been
written yet. Accepted temporarily to prioritize documentation while context
was fresh (see `docs/exec-plans/active/0001-bootstrap-repo-and-core-loop.md`).
**Trigger to fix:** before merging any change that touches more than one
layer.

## No CI pipeline yet

`.github/workflows/ci.yml` doesn't exist yet. **Trigger to fix:** before
relying on PR-based review/merge for this repo.

## Duplicate artist names are allowed

`ArtistCatalogService.AddArtistAsync` has no uniqueness check. Accepted for
the MVP since it's a personal catalog tool. **Trigger to fix:** if it turns
out to be annoying in practice, or once tagging/discography make duplicate
entries actually ambiguous (e.g. two different "Autechre" rows with
different tags).
