using System.Text;

namespace FileHashComparator.App.Services;

public static class HashFormatting
{
    private const string HexAlphabet = "0123456789ABCDEF";

    public static string ToHex(ReadOnlySpan<byte> bytes)
    {
        var result = new char[bytes.Length * 2];
        var index = 0;
        foreach (var b in bytes)
        {
            result[index++] = HexAlphabet[b >> 4];
            result[index++] = HexAlphabet[b & 0x0F];
        }
        return new string(result);
    }

    public static bool TryNormalizeHex(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsWhiteSpace(ch) || ch == '-')
            {
                continue;
            }

            if (!Uri.IsHexDigit(ch))
            {
                return false;
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        if (builder.Length == 0)
        {
            return false;
        }

        normalized = builder.ToString();
        return true;
    }
}

