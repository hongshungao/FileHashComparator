namespace FileHashComparator.App.Services.Hashing;

public sealed class Crc32Hash : IStreamingHash
{
    private readonly uint[] _table;
    private uint _crc;

    public Crc32Hash(uint polynomial)
    {
        _table = BuildTable(polynomial);
        _crc = 0xFFFFFFFF;
    }

    public int HashSizeBytes => 4;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            var index = (byte)(_crc ^ value);
            _crc = (_crc >> 8) ^ _table[index];
        }
    }

    public byte[] GetHashAndReset()
    {
        var result = ~_crc;
        _crc = 0xFFFFFFFF;
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

    private static uint[] BuildTable(uint polynomial)
    {
        var table = new uint[256];
        for (var i = 0; i < table.Length; i++)
        {
            uint crc = (uint)i;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
            table[i] = crc;
        }

        return table;
    }
}
