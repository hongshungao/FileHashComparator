using System.Buffers.Binary;

namespace FileHashComparator.App.Services.Hashing;

public sealed class XxHash64Hash : IStreamingHash
{
    private const ulong Prime1 = 0x9E3779B185EBCA87;
    private const ulong Prime2 = 0xC2B2AE3D27D4EB4F;
    private const ulong Prime3 = 0x165667B19E3779F9;
    private const ulong Prime4 = 0x85EBCA77C2B2AE63;
    private const ulong Prime5 = 0x27D4EB2F165667C5;

    private readonly byte[] _buffer = new byte[32];
    private int _bufferLength;
    private ulong _v1;
    private ulong _v2;
    private ulong _v3;
    private ulong _v4;
    private ulong _totalLength;

    public XxHash64Hash(ulong seed = 0)
    {
        Seed = seed;
        ResetState();
    }

    public ulong Seed { get; }

    public int HashSizeBytes => 8;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        _totalLength += (ulong)data.Length;
        var offset = 0;

        if (_bufferLength + data.Length < 32)
        {
            data.CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += data.Length;
            return;
        }

        if (_bufferLength > 0)
        {
            var toCopy = 32 - _bufferLength;
            data.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            ProcessChunk(_buffer);
            _bufferLength = 0;
            offset += toCopy;
        }

        while (offset + 32 <= data.Length)
        {
            ProcessChunk(data.Slice(offset, 32));
            offset += 32;
        }

        if (offset < data.Length)
        {
            data.Slice(offset).CopyTo(_buffer);
            _bufferLength = data.Length - offset;
        }
    }

    public byte[] GetHashAndReset()
    {
        var hash = _totalLength >= 32
            ? RotateLeft(_v1, 1) + RotateLeft(_v2, 7) + RotateLeft(_v3, 12) + RotateLeft(_v4, 18)
            : Seed + Prime5;

        hash += _totalLength;

        var remaining = _buffer.AsSpan(0, _bufferLength);
        var offset = 0;
        while (offset + 8 <= remaining.Length)
        {
            var lane = BinaryPrimitives.ReadUInt64LittleEndian(remaining.Slice(offset, 8));
            hash ^= Round(0, lane);
            hash = RotateLeft(hash, 27) * Prime1 + Prime4;
            offset += 8;
        }

        if (offset + 4 <= remaining.Length)
        {
            hash ^= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(remaining.Slice(offset, 4)) * Prime1;
            hash = RotateLeft(hash, 23) * Prime2 + Prime3;
            offset += 4;
        }

        while (offset < remaining.Length)
        {
            hash ^= remaining[offset] * Prime5;
            hash = RotateLeft(hash, 11) * Prime1;
            offset++;
        }

        hash ^= hash >> 33;
        hash *= Prime2;
        hash ^= hash >> 29;
        hash *= Prime3;
        hash ^= hash >> 32;

        ResetState();
        return new[]
        {
            (byte)(hash >> 56),
            (byte)(hash >> 48),
            (byte)(hash >> 40),
            (byte)(hash >> 32),
            (byte)(hash >> 24),
            (byte)(hash >> 16),
            (byte)(hash >> 8),
            (byte)hash
        };
    }

    public void Dispose()
    {
    }

    private void ResetState()
    {
        _v1 = Seed + Prime1 + Prime2;
        _v2 = Seed + Prime2;
        _v3 = Seed;
        _v4 = Seed - Prime1;
        _totalLength = 0;
        _bufferLength = 0;
    }

    private void ProcessChunk(ReadOnlySpan<byte> chunk)
    {
        _v1 = Round(_v1, BinaryPrimitives.ReadUInt64LittleEndian(chunk.Slice(0, 8)));
        _v2 = Round(_v2, BinaryPrimitives.ReadUInt64LittleEndian(chunk.Slice(8, 8)));
        _v3 = Round(_v3, BinaryPrimitives.ReadUInt64LittleEndian(chunk.Slice(16, 8)));
        _v4 = Round(_v4, BinaryPrimitives.ReadUInt64LittleEndian(chunk.Slice(24, 8)));
    }

    private static ulong Round(ulong acc, ulong lane)
    {
        acc += lane * Prime2;
        acc = RotateLeft(acc, 31);
        acc *= Prime1;
        return acc;
    }

    private static ulong RotateLeft(ulong value, int count) => (value << count) | (value >> (64 - count));
}
