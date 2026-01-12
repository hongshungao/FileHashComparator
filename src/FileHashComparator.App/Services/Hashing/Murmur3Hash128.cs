using System.Buffers.Binary;

namespace FileHashComparator.App.Services.Hashing;

public sealed class Murmur3Hash128 : IStreamingHash
{
    private const ulong C1 = 0x87c37b91114253d5;
    private const ulong C2 = 0x4cf5ad432745937f;

    private readonly byte[] _buffer = new byte[16];
    private int _bufferLength;
    private ulong _totalLength;
    private ulong _h1;
    private ulong _h2;

    public Murmur3Hash128(ulong seed = 0)
    {
        Seed = seed;
        _h1 = seed;
        _h2 = seed;
    }

    public ulong Seed { get; }

    public int HashSizeBytes => 16;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        _totalLength += (ulong)data.Length;
        var offset = 0;

        if (_bufferLength > 0)
        {
            var toCopy = Math.Min(16 - _bufferLength, data.Length);
            data.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += toCopy;
            offset += toCopy;

            if (_bufferLength == 16)
            {
                ProcessBlock(_buffer);
                _bufferLength = 0;
            }
        }

        while (offset + 16 <= data.Length)
        {
            ProcessBlock(data.Slice(offset, 16));
            offset += 16;
        }

        if (offset < data.Length)
        {
            data.Slice(offset).CopyTo(_buffer);
            _bufferLength = data.Length - offset;
        }
    }

    public byte[] GetHashAndReset()
    {
        ulong k1 = 0;
        ulong k2 = 0;

        switch (_bufferLength)
        {
            case 15:
                k2 ^= (ulong)_buffer[14] << 48;
                goto case 14;
            case 14:
                k2 ^= (ulong)_buffer[13] << 40;
                goto case 13;
            case 13:
                k2 ^= (ulong)_buffer[12] << 32;
                goto case 12;
            case 12:
                k2 ^= (ulong)_buffer[11] << 24;
                goto case 11;
            case 11:
                k2 ^= (ulong)_buffer[10] << 16;
                goto case 10;
            case 10:
                k2 ^= (ulong)_buffer[9] << 8;
                goto case 9;
            case 9:
                k2 ^= _buffer[8];
                k2 *= C2;
                k2 = RotateLeft(k2, 33);
                k2 *= C1;
                _h2 ^= k2;
                break;
        }

        switch (_bufferLength)
        {
            case 8:
                k1 ^= (ulong)_buffer[7] << 56;
                goto case 7;
            case 7:
                k1 ^= (ulong)_buffer[6] << 48;
                goto case 6;
            case 6:
                k1 ^= (ulong)_buffer[5] << 40;
                goto case 5;
            case 5:
                k1 ^= (ulong)_buffer[4] << 32;
                goto case 4;
            case 4:
                k1 ^= (ulong)_buffer[3] << 24;
                goto case 3;
            case 3:
                k1 ^= (ulong)_buffer[2] << 16;
                goto case 2;
            case 2:
                k1 ^= (ulong)_buffer[1] << 8;
                goto case 1;
            case 1:
                k1 ^= _buffer[0];
                k1 *= C1;
                k1 = RotateLeft(k1, 31);
                k1 *= C2;
                _h1 ^= k1;
                break;
        }

        _h1 ^= _totalLength;
        _h2 ^= _totalLength;

        _h1 += _h2;
        _h2 += _h1;

        _h1 = Fmix(_h1);
        _h2 = Fmix(_h2);

        _h1 += _h2;
        _h2 += _h1;

        var result = new byte[16];
        WriteUInt64(result, 0, _h1);
        WriteUInt64(result, 8, _h2);

        Reset();
        return result;
    }

    public void Dispose()
    {
    }

    private void Reset()
    {
        _bufferLength = 0;
        _totalLength = 0;
        _h1 = Seed;
        _h2 = Seed;
    }

    private void ProcessBlock(ReadOnlySpan<byte> block)
    {
        var k1 = BinaryPrimitives.ReadUInt64LittleEndian(block.Slice(0, 8));
        var k2 = BinaryPrimitives.ReadUInt64LittleEndian(block.Slice(8, 8));

        k1 *= C1;
        k1 = RotateLeft(k1, 31);
        k1 *= C2;
        _h1 ^= k1;

        _h1 = RotateLeft(_h1, 27);
        _h1 += _h2;
        _h1 = _h1 * 5 + 0x52dce729;

        k2 *= C2;
        k2 = RotateLeft(k2, 33);
        k2 *= C1;
        _h2 ^= k2;

        _h2 = RotateLeft(_h2, 31);
        _h2 += _h1;
        _h2 = _h2 * 5 + 0x38495ab5;
    }

    private static ulong RotateLeft(ulong value, int count) => (value << count) | (value >> (64 - count));

    private static ulong Fmix(ulong value)
    {
        value ^= value >> 33;
        value *= 0xff51afd7ed558ccd;
        value ^= value >> 33;
        value *= 0xc4ceb9fe1a85ec53;
        value ^= value >> 33;
        return value;
    }

    private static void WriteUInt64(byte[] buffer, int offset, ulong value)
    {
        buffer[offset] = (byte)(value >> 56);
        buffer[offset + 1] = (byte)(value >> 48);
        buffer[offset + 2] = (byte)(value >> 40);
        buffer[offset + 3] = (byte)(value >> 32);
        buffer[offset + 4] = (byte)(value >> 24);
        buffer[offset + 5] = (byte)(value >> 16);
        buffer[offset + 6] = (byte)(value >> 8);
        buffer[offset + 7] = (byte)value;
    }
}
