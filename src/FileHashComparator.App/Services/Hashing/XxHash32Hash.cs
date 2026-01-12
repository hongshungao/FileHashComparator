using System.Buffers.Binary;

namespace FileHashComparator.App.Services.Hashing;

public sealed class XxHash32Hash : IStreamingHash
{
    private const uint Prime1 = 0x9E3779B1;
    private const uint Prime2 = 0x85EBCA77;
    private const uint Prime3 = 0xC2B2AE3D;
    private const uint Prime4 = 0x27D4EB2F;
    private const uint Prime5 = 0x165667B1;

    private readonly byte[] _buffer = new byte[16];
    private int _bufferLength;
    private uint _v1;
    private uint _v2;
    private uint _v3;
    private uint _v4;
    private ulong _totalLength;

    public XxHash32Hash(uint seed = 0)
    {
        Seed = seed;
        ResetState();
    }

    public uint Seed { get; }

    public int HashSizeBytes => 4;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        _totalLength += (ulong)data.Length;
        var offset = 0;

        if (_bufferLength + data.Length < 16)
        {
            data.CopyTo(_buffer.AsSpan(_bufferLength));
            _bufferLength += data.Length;
            return;
        }

        if (_bufferLength > 0)
        {
            var toCopy = 16 - _bufferLength;
            data.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferLength));
            ProcessChunk(_buffer);
            _bufferLength = 0;
            offset += toCopy;
        }

        while (offset + 16 <= data.Length)
        {
            ProcessChunk(data.Slice(offset, 16));
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
        var hash = _totalLength >= 16
            ? RotateLeft(_v1, 1) + RotateLeft(_v2, 7) + RotateLeft(_v3, 12) + RotateLeft(_v4, 18)
            : Seed + Prime5;

        hash += (uint)_totalLength;

        var remaining = _buffer.AsSpan(0, _bufferLength);
        var offset = 0;
        while (offset + 4 <= remaining.Length)
        {
            var lane = BinaryPrimitives.ReadUInt32LittleEndian(remaining.Slice(offset, 4));
            hash += lane * Prime3;
            hash = RotateLeft(hash, 17) * Prime4;
            offset += 4;
        }

        while (offset < remaining.Length)
        {
            hash += remaining[offset] * Prime5;
            hash = RotateLeft(hash, 11) * Prime1;
            offset++;
        }

        hash ^= hash >> 15;
        hash *= Prime2;
        hash ^= hash >> 13;
        hash *= Prime3;
        hash ^= hash >> 16;

        ResetState();
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
        _v1 = Round(_v1, BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(0, 4)));
        _v2 = Round(_v2, BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(4, 4)));
        _v3 = Round(_v3, BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(8, 4)));
        _v4 = Round(_v4, BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(12, 4)));
    }

    private static uint Round(uint acc, uint lane)
    {
        acc += lane * Prime2;
        acc = RotateLeft(acc, 13);
        acc *= Prime1;
        return acc;
    }

    private static uint RotateLeft(uint value, int count) => (value << count) | (value >> (32 - count));
}
