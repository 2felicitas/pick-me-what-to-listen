namespace PickMeWhatToListen.Domain;

public static class MetadataTermNormalizer
{
    public static string ToComparisonKey(string name) => name.Trim().ToLowerInvariant();
}
