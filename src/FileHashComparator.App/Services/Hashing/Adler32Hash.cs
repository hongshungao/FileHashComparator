namespace FileHashComparator.App.Services.Hashing;

public sealed class Adler32Hash : IStreamingHash
{
    private const uint ModAdler = 65521;
    private uint _a = 1;
    private uint _b;

    public int HashSizeBytes => 4;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            _a = (_a + value) % ModAdler;
            _b = (_b + _a) % ModAdler;
        }
    }

    public byte[] GetHashAndReset()
    {
        var result = (_b << 16) | _a;
        _a = 1;
        _b = 0;
        return new[]
        {
            (byte)(result >> 24),
            (byte)(result >> 16),
            (byte)(result >> 8),
            (byte)result
        };
    }

    public void Dispose()
    {
    }
}
