namespace FileHashComparator.App.Services.Hashing;

public sealed class Crc64EcmaHash : IStreamingHash
{
    private const ulong Polynomial = 0x42F0E1EBA9EA3693;
    private readonly ulong[] _table;
    private ulong _crc;

    public Crc64EcmaHash()
    {
        _table = BuildTable();
        _crc = 0;
    }

    public int HashSizeBytes => 8;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            var index = (byte)((_crc >> 56) ^ value);
            _crc = _table[index] ^ (_crc << 8);
        }
    }

    public byte[] GetHashAndReset()
    {
        var result = _crc;
        _crc = 0;
        return new[]
        {
            (byte)(result >> 56),
            (byte)(result >> 48),
            (byte)(result >> 40),
            (byte)(result >> 32),
            (byte)(result >> 24),
            (byte)(result >> 16),
            (byte)(result >> 8),
            (byte)result
        };
    }

    public void Dispose()
    {
    }

    private static ulong[] BuildTable()
    {
        var table = new ulong[256];
        for (ulong i = 0; i < 256; i++)
        {
            var crc = i << 56;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 0x8000000000000000) != 0 ? (crc << 1) ^ Polynomial : crc << 1;
            }
            table[i] = crc;
        }

        return table;
    }
}
