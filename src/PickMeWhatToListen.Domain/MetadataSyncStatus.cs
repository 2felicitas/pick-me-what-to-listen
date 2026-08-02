namespace PickMeWhatToListen.Domain;

public enum MetadataSyncStatus
{
    None = 0,
    Ok,
    Ambiguous,
    NotFound,
    Failed,
}
