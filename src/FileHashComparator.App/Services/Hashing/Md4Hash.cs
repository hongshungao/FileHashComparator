namespace FileHashComparator.App.Services.Hashing;

public sealed class Md4Hash : IStreamingHash
{
    private readonly byte[] _buffer = new byte[64];
    private int _bufferLength;
    private ulong _totalBytes;

    private uint _a = 0x67452301;
    private uint _b = 0xefcdab89;
    private uint _c = 0x98badcfe;
    private uint _d = 0x10325476;

    public int HashSizeBytes => 16;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        var offset = 0;
        _totalBytes += (ulong)data.Length;

        if (_bufferLength > 0)
        {
            var toCopy = Math.Min(64 - _bufferLength, data.Length);
            data.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += toCopy;
            offset += toCopy;

            if (_bufferLength == 64)
            {
                ProcessBlock(_buffer);
                _bufferLength = 0;
            }
        }

        while (offset + 64 <= data.Length)
        {
            ProcessBlock(data.Slice(offset, 64));
            offset += 64;
        }

        if (offset < data.Length)
        {
            data.Slice(offset).CopyTo(_buffer);
            _bufferLength = data.Length - offset;
        }
    }

    public byte[] GetHashAndReset()
    {
        var bitLength = _totalBytes * 8;

        Span<byte> padding = stackalloc byte[64];
        padding[0] = 0x80;

        var paddingLength = _bufferLength < 56 ? 56 - _bufferLength : 120 - _bufferLength;
        AppendData(padding.Slice(0, paddingLength));

        Span<byte> lengthBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(lengthBytes, bitLength);
        AppendData(lengthBytes);

        var result = new byte[16];
        WriteUInt32(result, 0, _a);
        WriteUInt32(result, 4, _b);
        WriteUInt32(result, 8, _c);
        WriteUInt32(result, 12, _d);

        Reset();
        return result;
    }

    public void Dispose()
    {
    }

    private void Reset()
    {
        _bufferLength = 0;
        _totalBytes = 0;
        _a = 0x67452301;
        _b = 0xefcdab89;
        _c = 0x98badcfe;
        _d = 0x10325476;
        Array.Clear(_buffer);
    }

    private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);
    private static uint G(uint x, uint y, uint z) => (x & y) | (x & z) | (y & z);
    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

    private static uint RotateLeft(uint value, int bits) => (value << bits) | (value >> (32 - bits));

    private void ProcessBlock(ReadOnlySpan<byte> block)
    {
        Span<uint> x = stackalloc uint[16];
        for (var i = 0; i < 16; i++)
        {
            x[i] = BitConverter.ToUInt32(block.Slice(i * 4, 4));
        }

        var a = _a;
        var b = _b;
        var c = _c;
        var d = _d;

        // Round 1
        FF(ref a, b, c, d, x[0], 3);
        FF(ref d, a, b, c, x[1], 7);
        FF(ref c, d, a, b, x[2], 11);
        FF(ref b, c, d, a, x[3], 19);
        FF(ref a, b, c, d, x[4], 3);
        FF(ref d, a, b, c, x[5], 7);
        FF(ref c, d, a, b, x[6], 11);
        FF(ref b, c, d, a, x[7], 19);
        FF(ref a, b, c, d, x[8], 3);
        FF(ref d, a, b, c, x[9], 7);
        FF(ref c, d, a, b, x[10], 11);
        FF(ref b, c, d, a, x[11], 19);
        FF(ref a, b, c, d, x[12], 3);
        FF(ref d, a, b, c, x[13], 7);
        FF(ref c, d, a, b, x[14], 11);
        FF(ref b, c, d, a, x[15], 19);

        // Round 2
        GG(ref a, b, c, d, x[0], 3);
        GG(ref d, a, b, c, x[4], 5);
        GG(ref c, d, a, b, x[8], 9);
        GG(ref b, c, d, a, x[12], 13);
        GG(ref a, b, c, d, x[1], 3);
        GG(ref d, a, b, c, x[5], 5);
        GG(ref c, d, a, b, x[9], 9);
        GG(ref b, c, d, a, x[13], 13);
        GG(ref a, b, c, d, x[2], 3);
        GG(ref d, a, b, c, x[6], 5);
        GG(ref c, d, a, b, x[10], 9);
        GG(ref b, c, d, a, x[14], 13);
        GG(ref a, b, c, d, x[3], 3);
        GG(ref d, a, b, c, x[7], 5);
        GG(ref c, d, a, b, x[11], 9);
        GG(ref b, c, d, a, x[15], 13);

        // Round 3
        HH(ref a, b, c, d, x[0], 3);
        HH(ref d, a, b, c, x[8], 9);
        HH(ref c, d, a, b, x[4], 11);
        HH(ref b, c, d, a, x[12], 15);
        HH(ref a, b, c, d, x[2], 3);
        HH(ref d, a, b, c, x[10], 9);
        HH(ref c, d, a, b, x[6], 11);
        HH(ref b, c, d, a, x[14], 15);
        HH(ref a, b, c, d, x[1], 3);
        HH(ref d, a, b, c, x[9], 9);
        HH(ref c, d, a, b, x[5], 11);
        HH(ref b, c, d, a, x[13], 15);
        HH(ref a, b, c, d, x[3], 3);
        HH(ref d, a, b, c, x[11], 9);
        HH(ref c, d, a, b, x[7], 11);
        HH(ref b, c, d, a, x[15], 15);

        _a += a;
        _b += b;
        _c += c;
        _d += d;
    }

    private void FF(ref uint a, uint b, uint c, uint d, uint x, int s)
    {
        a = RotateLeft(a + F(b, c, d) + x, s);
    }

    private void GG(ref uint a, uint b, uint c, uint d, uint x, int s)
    {
        a = RotateLeft(a + G(b, c, d) + x + 0x5a827999, s);
    }

    private void HH(ref uint a, uint b, uint c, uint d, uint x, int s)
    {
        a = RotateLeft(a + H(b, c, d) + x + 0x6ed9eba1, s);
    }

    private static void WriteUInt32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
