using System.Buffers.Binary;

namespace FileHashComparator.App.Services.Hashing;

public sealed class Murmur3Hash32 : IStreamingHash
{
    private const uint C1 = 0xcc9e2d51;
    private const uint C2 = 0x1b873593;

    private readonly byte[] _buffer = new byte[4];
    private int _bufferLength;
    private ulong _totalLength;
    private uint _h1;

    public Murmur3Hash32(uint seed = 0)
    {
        Seed = seed;
        _h1 = seed;
    }

    public uint Seed { get; }

    public int HashSizeBytes => 4;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        _totalLength += (ulong)data.Length;
        var offset = 0;

        if (_bufferLength > 0)
        {
            var toCopy = Math.Min(4 - _bufferLength, data.Length);
            data.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += toCopy;
            offset += toCopy;

            if (_bufferLength == 4)
            {
                ProcessBlock(_buffer);
                _bufferLength = 0;
            }
        }

        while (offset + 4 <= data.Length)
        {
            ProcessBlock(data.Slice(offset, 4));
            offset += 4;
        }

        if (offset < data.Length)
        {
            data.Slice(offset).CopyTo(_buffer);
            _bufferLength = data.Length - offset;
        }
    }

    public byte[] GetHashAndReset()
    {
        uint k1 = 0;
        switch (_bufferLength)
        {
            case 3:
                k1 ^= (uint)_buffer[2] << 16;
                goto case 2;
            case 2:
                k1 ^= (uint)_buffer[1] << 8;
                goto case 1;
            case 1:
                k1 ^= _buffer[0];
                k1 *= C1;
                k1 = RotateLeft(k1, 15);
                k1 *= C2;
                _h1 ^= k1;
                break;
        }

        _h1 ^= (uint)_totalLength;
        _h1 = Fmix(_h1);

        var hash = _h1;
        Reset();
        return new[]
        {
            (byte)(hash >> 24),
            (byte)(hash >> 16),
            (byte)(hash >> 8),
            (byte)hash
        };
    }

    public void Dispose()
    {
    }

    private void Reset()
    {
        _bufferLength = 0;
        _totalLength = 0;
        _h1 = Seed;
    }

    private void ProcessBlock(ReadOnlySpan<byte> block)
    {
        var k1 = BinaryPrimitives.ReadUInt32LittleEndian(block);
        k1 *= C1;
        k1 = RotateLeft(k1, 15);
        k1 *= C2;

        _h1 ^= k1;
        _h1 = RotateLeft(_h1, 13);
        _h1 = _h1 * 5 + 0xe6546b64;
    }

    private static uint RotateLeft(uint value, int count) => (value << count) | (value >> (32 - count));

    private static uint Fmix(uint value)
    {
        value ^= value >> 16;
        value *= 0x85ebca6b;
        value ^= value >> 13;
        value *= 0xc2b2ae35;
        value ^= value >> 16;
        return value;
    }
}
