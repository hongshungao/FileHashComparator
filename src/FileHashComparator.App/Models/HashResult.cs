namespace FileHashComparator.App.Models;

public sealed class HashResult
{
    public HashResult(bool success, string? hashHex, string? error)
    {
        Success = success;
        HashHex = hashHex;
        Error = error;
    }

    public bool Success { get; }
    public string? HashHex { get; }
    public string? Error { get; }
}
