namespace FileHashComparator.App.Models;

public sealed class HashListEntry
{
    public HashListEntry(int lineNumber, string rawLine, string? path, string? hash, string? error)
    {
        LineNumber = lineNumber;
        RawLine = rawLine;
        Path = path;
        Hash = hash;
        Error = error;
    }

    public int LineNumber { get; }
    public string RawLine { get; }
    public string? Path { get; }
    public string? Hash { get; }
    public string? Error { get; }

    public bool IsValid => Error is null;
}
