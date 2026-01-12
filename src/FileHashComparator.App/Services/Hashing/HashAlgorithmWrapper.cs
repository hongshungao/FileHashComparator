using System.Security.Cryptography;

namespace FileHashComparator.App.Services.Hashing;

public sealed class HashAlgorithmWrapper : IStreamingHash
{
    private readonly HashAlgorithm _hashAlgorithm;
    private bool _finalized;

    public HashAlgorithmWrapper(HashAlgorithm hashAlgorithm)
    {
        _hashAlgorithm = hashAlgorithm;
    }

    public int HashSizeBytes => _hashAlgorithm.HashSize / 8;

    public void AppendData(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        var buffer = data.ToArray();
        _hashAlgorithm.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
    }

    public byte[] GetHashAndReset()
    {
        if (!_finalized)
        {
            _hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            _finalized = true;
        }

        var hash = _hashAlgorithm.Hash ?? Array.Empty<byte>();
        _hashAlgorithm.Initialize();
        _finalized = false;
        return hash;
    }

    public void Dispose()
    {
        _hashAlgorithm.Dispose();
    }
}
