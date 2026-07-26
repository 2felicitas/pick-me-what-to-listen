# Tech debt tracker

Known, deliberately-deferred issues. Each entry should say what the debt is,
why it was accepted, and what would trigger fixing it.

## Duplicate artist names are allowed

`ArtistCatalogService.AddArtistAsync` has no uniqueness check. Accepted for
the MVP since it's a personal catalog tool. **Trigger to fix:** if it turns
out to be annoying in practice, or once tagging/discography make duplicate
entries actually ambiguous (e.g. two different "Autechre" rows with
different tags).
