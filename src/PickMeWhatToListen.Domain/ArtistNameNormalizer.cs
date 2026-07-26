using System.Globalization;
using System.Text;

namespace PickMeWhatToListen.Domain;

/// <summary>
/// Computes a comparison key used to detect duplicate artist names that
/// differ only in whitespace, case, apostrophe style, or diacritics — e.g.
/// "Мёбиус"/"Мебиус" (ё/е) or "Şevval"/"Sevval" (ş/s) both fold to the same
/// key, since both pairs are canonically equivalent once diacritics are
/// stripped (Unicode NFD decomposes them into a base letter + a combining
/// mark). This does <b>not</b> unify non-diacritic look-alikes — Turkish
/// dotless "ı" vs "i", German "ß" vs "ss", "ø" vs "o" — since those aren't
/// decomposable the same way; see `ArtistNameNormalizerTests` for the exact
/// set of cases this covers. See `docs/product-specs/bulk-import.md`.
/// </summary>
/// <remarks>
/// Only ever used for comparison — never rewrites what gets stored as
/// <see cref="Artist.Name"/>.
/// </remarks>
public static class ArtistNameNormalizer
{
    public static string ToComparisonKey(string name)
    {
        var collapsedWhitespace = CollapseWhitespace(name.Trim());
        var decomposed = collapsedWhitespace.Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (IsApostropheVariant(ch) || CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString().ToLowerInvariant();
    }

    private static bool IsApostropheVariant(char ch) =>
        ch is '\'' or '’' or '‘' or '`' or '´';

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
            }
            else
            {
                builder.Append(ch);
                previousWasWhitespace = false;
            }
        }

        return builder.ToString();
    }
}
