namespace colibri_api.Common.Persistence;

public enum SaveResult
{
    Success,
    ConcurrencyConflict,
    DuplicateKey,
}
