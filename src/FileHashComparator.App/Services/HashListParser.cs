using FileHashComparator.App.Models;
using System.IO;

namespace FileHashComparator.App.Services;

public sealed class HashListParser
{
    public IReadOnlyList<HashListEntry> Parse(string filePath)
    {
        var results = new List<HashListEntry>();
        var lineNumber = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var splitIndex = FindFirstWhitespace(trimmed);
            if (splitIndex <= 0)
            {
                results.Add(new HashListEntry(lineNumber, line, null, null, "Invalid format"));
                continue;
            }

            var hashPart = trimmed[..splitIndex].Trim();
            var pathPart = trimmed[(splitIndex + 1)..].TrimStart();
            if (pathPart.StartsWith("*", StringComparison.Ordinal))
            {
                pathPart = pathPart[1..].TrimStart();
            }

            if (string.IsNullOrWhiteSpace(hashPart) || string.IsNullOrWhiteSpace(pathPart))
            {
                results.Add(new HashListEntry(lineNumber, line, null, null, "Invalid format"));
                continue;
            }

            results.Add(new HashListEntry(lineNumber, line, pathPart, hashPart, null));
        }

        return results;
    }

    private static int FindFirstWhitespace(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
