# Bulk import & duplicate detection

Status: In progress (see `docs/exec-plans/active/0002-bulk-import-and-duplicate-detection.md`)

## Goal

Let the user add many artists at once from a text file, and stop the
catalog from silently accumulating the same artist twice under
superficially different spellings.

## Behavior

- **Duplicate detection** applies to both the existing single "Add artist"
  flow and the new bulk import. Two names are considered the same artist
  if they're equal after: trimming, collapsing internal whitespace,
  case-insensitive comparison, and Unicode canonical (NFD) decomposition
  with combining marks stripped. This unifies e.g. "Мёбиус"/"Мебиус" (ё/е)
  and "Şevval"/"Sevval" (ş/s) automatically, since both pairs are
  canonically equivalent once diacritics are removed — no hand-maintained
  lookup table needed.
  - **Known limitation:** this only catches diacritic-related variants.
    Non-diacritic look-alikes (Turkish dotless "ı" vs "i", German "ß" vs
    "ss", "ø" vs "o") are *not* unified by this approach — that would need
    an explicit mapping table, not attempted in this pass. See
    `ArtistNameNormalizerTests` for the exact set of cases covered.
  - The stored `Artist.Name` always keeps whatever casing/spelling the
    user typed — normalization is only used for comparison, never to
    rewrite what gets saved.
- **Single add**: if the normalized name already matches an existing
  catalog entry, reject the add and show a message naming the existing
  entry; nothing new is added.
- **Bulk import**: the user picks a `.txt` file (one artist name per line)
  via a native file picker. Blank lines are skipped silently. Each
  remaining line goes through the same validation as a single add (trim,
  max 200 chars) and the same duplicate check — both against the existing
  catalog and against other lines already accepted earlier in the same
  file (first occurrence in the file wins). Invalid and duplicate lines
  are skipped, not fatal to the rest of the import — it always finishes
  and reports a one-line summary of how many were added vs. skipped.

## Non-goals (for this spec)

- Showing *which* specific lines were skipped and why, beyond a total
  count — see `docs/exec-plans/tech-debt-tracker.md` if this turns out to
  be needed later.
- Any file format other than plain text, one name per line (no CSV,
  no per-line metadata/tags).
- A preview/confirm step before the import commits — it applies
  immediately, same as the single-add flow.
- Unifying non-diacritic look-alike letters (see limitation above).

## Domain model

`PickMeWhatToListen.Domain.ArtistNameNormalizer.ToComparisonKey(string)` is
a pure function with no persistence impact — see `ARCHITECTURE.md` for
where it sits relative to `Artist`. No schema change: duplicate checking
compares against `Artist.Name` values already in the table, it doesn't add
a new indexed/normalized column.
