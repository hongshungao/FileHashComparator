namespace FileHashComparator.App.Services.Hashing;

public sealed class Md2Hash : IStreamingHash
{
    private static readonly byte[] S =
    {
        41, 46, 67, 201, 162, 216, 124, 1, 61, 54, 84, 161, 236, 240, 6, 19,
        98, 167, 5, 243, 192, 199, 115, 140, 152, 147, 43, 217, 188, 76, 130, 202,
        30, 155, 87, 60, 253, 212, 224, 22, 103, 66, 111, 24, 138, 23, 229, 18,
        190, 78, 196, 214, 218, 158, 222, 73, 160, 251, 245, 142, 187, 47, 238, 122,
        169, 104, 121, 145, 21, 178, 7, 63, 148, 194, 16, 137, 11, 34, 95, 33,
        128, 127, 93, 154, 90, 144, 50, 39, 53, 62, 204, 231, 191, 247, 151, 3,
        255, 25, 48, 179, 72, 165, 181, 209, 215, 94, 146, 42, 172, 86, 170, 198,
        79, 184, 56, 210, 150, 164, 125, 182, 118, 252, 107, 226, 156, 116, 4, 241,
        69, 157, 112, 89, 100, 113, 135, 32, 134, 91, 207, 101, 230, 45, 168, 2,
        27, 96, 37, 173, 174, 176, 185, 246, 28, 70, 97, 105, 52, 64, 126, 15,
        85, 71, 163, 35, 221, 81, 175, 58, 195, 92, 249, 206, 186, 197, 234, 38,
        44, 83, 13, 110, 133, 40, 132, 9, 211, 223, 205, 244, 65, 129, 77, 82,
        106, 220, 55, 200, 108, 193, 171, 250, 36, 225, 123, 8, 12, 189, 177, 74,
        120, 136, 149, 139, 227, 99, 232, 109, 233, 203, 213, 254, 59, 0, 29, 57,
        242, 239, 183, 14, 102, 88, 208, 228, 166, 119, 114, 248, 235, 117, 75, 10,
        49, 68, 80, 180, 143, 237, 31, 26, 219, 153, 141, 51, 159, 17, 131, 20
    };

    private readonly byte[] _state = new byte[16];
    private readonly byte[] _checksum = new byte[16];
    private readonly byte[] _buffer = new byte[16];
    private int _bufferLength;

    public int HashSizeBytes => 16;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        var offset = 0;
        while (offset < data.Length)
        {
            var toCopy = Math.Min(16 - _bufferLength, data.Length - offset);
            data.Slice(offset, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += toCopy;
            offset += toCopy;

            if (_bufferLength == 16)
            {
                ProcessBlock(_buffer);
                UpdateChecksum(_buffer);
                _bufferLength = 0;
            }
        }
    }

    public byte[] GetHashAndReset()
    {
        var paddingLength = 16 - _bufferLength;
        var padding = new byte[paddingLength];
        for (var i = 0; i < padding.Length; i++)
        {
            padding[i] = (byte)paddingLength;
        }

        AppendData(padding);
        ProcessBlock(_checksum);

        var result = _state.ToArray();
        Array.Clear(_state);
        Array.Clear(_checksum);
        Array.Clear(_buffer);
        _bufferLength = 0;
        return result;
    }

    public void Dispose()
    {
    }

    private void ProcessBlock(ReadOnlySpan<byte> block)
    {
        Span<byte> x = stackalloc byte[48];
        for (var i = 0; i < 16; i++)
        {
            x[i] = _state[i];
            x[i + 16] = block[i];
            x[i + 32] = (byte)(_state[i] ^ block[i]);
        }

        byte t = 0;
        for (var i = 0; i < 18; i++)
        {
            for (var j = 0; j < 48; j++)
            {
                x[j] = (byte)(x[j] ^ S[t]);
                t = x[j];
            }
            t = (byte)(t + i);
        }

        x[..16].CopyTo(_state);
    }

    private void UpdateChecksum(ReadOnlySpan<byte> block)
    {
        byte t = _checksum[15];
        for (var i = 0; i < 16; i++)
        {
            _checksum[i] = (byte)(_checksum[i] ^ S[block[i] ^ t]);
            t = _checksum[i];
        }
    }
}
