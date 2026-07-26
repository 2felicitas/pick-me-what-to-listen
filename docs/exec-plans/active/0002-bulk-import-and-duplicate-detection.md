# 0002-bulk-import-and-duplicate-detection

Status: Active
Spec: [docs/product-specs/bulk-import.md](../../product-specs/bulk-import.md)
      (also amends [docs/product-specs/core-loop.md](../../product-specs/core-loop.md) —
      "Add artist" behavior)

## Goal

Add normalized (diacritic/case/apostrophe-insensitive) duplicate detection
to the existing single "Add artist" flow, and a new bulk-import-from-file
flow that reuses the same duplicate check.

## Scope

- In: `ArtistNameNormalizer` (Domain), duplicate check + `ArtistAddResult`
  on `ArtistCatalogService.AddArtistAsync`, a new
  `ArtistCatalogService.AddArtistsAsync` bulk method + `BulkAddArtistsResult`,
  WPF wiring (duplicate message on single add, "Import from file..." button
  + native file picker + summary message).
- Out (see `docs/product-specs/bulk-import.md` non-goals): per-line skip
  reasons in the UI, non-`.txt` formats, unifying non-diacritic look-alike
  letters (dotless ı, ß, ø).

## Plan

- [x] `ArtistNameNormalizer.ToComparisonKey` in Domain + `Domain.Tests`
      covering ё/е, ş/s, apostrophe variants, case/whitespace, and the
      documented non-diacritic limitation (dotless ı vs i NOT unified).
- [x] `ArtistAddResult` + duplicate check in
      `ArtistCatalogService.AddArtistAsync` + `Application.Tests` (using
      `FakeArtistRepository`).
- [x] `BulkAddArtistsResult` + `ArtistCatalogService.AddArtistsAsync` +
      `Application.Tests` (catalog dupes, in-file dupes, invalid lines,
      blank lines all counted/skipped correctly).
- [ ] `MainViewModel`: update `AddArtistCommand` to surface the duplicate
      message; add `ImportFromFileCommand` (native `OpenFileDialog`, reads
      the file, calls the new bulk method, shows the summary).
- [ ] `MainWindow.xaml`: "Import from file..." button.
- [ ] Manual smoke test (`dotnet run`) — no ViewModel/UI test coverage
      exists yet in this repo, see `.cursor/rules/testing.mdc`.
- [ ] Docs: flip `bulk-import.md` status to Implemented, move this plan to
      `completed/`.

## Decisions & deviations log

- **Flipped the repo-wide `InvariantGlobalization` default from `true` to
  `false`, superseding the per-project override decision in
  `0001-bootstrap-repo-and-core-loop.md`.** `ArtistNameNormalizer` relies on
  `string.Normalize(NormalizationForm.FormD)` to decompose e.g. "ё" into
  "е" + a combining mark it can then strip — but under invariant
  globalization mode, `string.Normalize` is a documented no-op (returns
  its input unchanged, no exception, no warning), so all the
  `Domain.Tests` cases with real diacritics initially failed silently
  wrong instead of erroring. `PickMeWhatToListen.Wpf.csproj` already
  overrode this to `false` for an unrelated reason (WPF's binding engine
  needs real culture data), meaning the actually-shipped app was already
  running non-invariant — so the repo-wide `true` default was providing no
  real footprint benefit, only a footgun for test hosts that don't
  override it. Removed the override from `Wpf.csproj` now that it matches
  the (new) repo default; updated `ARCHITECTURE.md` and
  `.cursor/rules/mvvm-wpf.mdc`/`csharp-style.mdc` accordingly.

## Open items / follow-ups

- <fill in when closing out>
