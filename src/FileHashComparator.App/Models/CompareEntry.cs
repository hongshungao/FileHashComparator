namespace FileHashComparator.App.Models;

public sealed class CompareEntry
{
    public CompareEntry(string path, string? expectedHash, string? actualHash, CompareStatus status, string statusText, string? message = null)
    {
        Path = path;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
        Status = status;
        StatusText = statusText;
        Message = message;
    }

    public string Path { get; }
    public string? ExpectedHash { get; }
    public string? ActualHash { get; }
    public CompareStatus Status { get; }
    public string StatusText { get; }
    public string? Message { get; }
}
