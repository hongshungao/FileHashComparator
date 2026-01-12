using FileHashComparator.App.Models;
using FileHashComparator.App.Services.Hashing;
using System.Security.Cryptography;

namespace FileHashComparator.App.Services;

public sealed class HashAlgorithmRegistry
{
    private readonly Dictionary<HashAlgorithmType, HashAlgorithmDefinition> _definitions;

    public HashAlgorithmRegistry()
    {
        _definitions = BuildDefinitions();
        Algorithms = _definitions.Values
            .Select(def =>
            {
                var instance = def.Factory();
                var supported = instance is not null;
                instance?.Dispose();
                return new HashAlgorithmInfo(def.Type, def.DisplayName, def.HashSizeBytes, supported, def.IsCryptographic);
            })
            .OrderBy(info => info.DisplayName)
            .ToList();
    }

    public IReadOnlyList<HashAlgorithmInfo> Algorithms { get; }

    public bool TryCreate(HashAlgorithmType type, out IStreamingHash? hash)
    {
        if (_definitions.TryGetValue(type, out var def))
        {
            hash = def.Factory();
            return hash is not null;
        }

        hash = null;
        return false;
    }

    public HashAlgorithmInfo? GetInfo(HashAlgorithmType type)
        => Algorithms.FirstOrDefault(a => a.Type == type);

    private static Dictionary<HashAlgorithmType, HashAlgorithmDefinition> BuildDefinitions()
    {
        var defs = new List<HashAlgorithmDefinition>
        {
            Crypto(HashAlgorithmType.Md2, "MD2", 16, () => new Md2Hash()),
            Crypto(HashAlgorithmType.Md4, "MD4", 16, () => new Md4Hash()),
            Crypto(HashAlgorithmType.Md5, "MD5", 16, () => new HashAlgorithmWrapper(MD5.Create())),
            Crypto(HashAlgorithmType.Sha1, "SHA-1", 20, () => new HashAlgorithmWrapper(SHA1.Create())),
            Crypto(HashAlgorithmType.Sha224, "SHA-224", 28, () => TryCreateByName("SHA-224")),
            Crypto(HashAlgorithmType.Sha256, "SHA-256", 32, () => new HashAlgorithmWrapper(SHA256.Create())),
            Crypto(HashAlgorithmType.Sha384, "SHA-384", 48, () => new HashAlgorithmWrapper(SHA384.Create())),
            Crypto(HashAlgorithmType.Sha512, "SHA-512", 64, () => new HashAlgorithmWrapper(SHA512.Create())),
            Crypto(HashAlgorithmType.Sha512_224, "SHA-512/224", 28, () => TryCreateByName("SHA-512/224")),
            Crypto(HashAlgorithmType.Sha512_256, "SHA-512/256", 32, () => TryCreateByName("SHA-512/256")),
            Crypto(HashAlgorithmType.Sha3_224, "SHA3-224", 28, () => TryCreateByName("SHA3-224")),
            Crypto(HashAlgorithmType.Sha3_256, "SHA3-256", 32, () => TryCreateByName("SHA3-256")),
            Crypto(HashAlgorithmType.Sha3_384, "SHA3-384", 48, () => TryCreateByName("SHA3-384")),
            Crypto(HashAlgorithmType.Sha3_512, "SHA3-512", 64, () => TryCreateByName("SHA3-512")),
            Crypto(HashAlgorithmType.Shake128, "SHAKE128", 32, () => TryCreateByName("SHAKE128")),
            Crypto(HashAlgorithmType.Shake256, "SHAKE256", 64, () => TryCreateByName("SHAKE256")),
            Crypto(HashAlgorithmType.RipeMd160, "RIPEMD-160", 20, () => TryCreateByName("RIPEMD160")),
            Crypto(HashAlgorithmType.Whirlpool, "Whirlpool", 64, () => TryCreateByName("Whirlpool")),
            Crypto(HashAlgorithmType.Blake2b, "BLAKE2b", 64, () => TryCreateByName("BLAKE2b")),
            Crypto(HashAlgorithmType.Blake2s, "BLAKE2s", 32, () => TryCreateByName("BLAKE2s")),
            Crypto(HashAlgorithmType.Blake3, "BLAKE3", 32, () => TryCreateByName("BLAKE3")),

            NonCrypto(HashAlgorithmType.Crc32, "CRC32", 4, () => new Crc32Hash(0xEDB88320)),
            NonCrypto(HashAlgorithmType.Crc32C, "CRC32C", 4, () => new Crc32Hash(0x82F63B78)),
            NonCrypto(HashAlgorithmType.Crc64Ecma, "CRC64-ECMA", 8, () => new Crc64EcmaHash()),
            NonCrypto(HashAlgorithmType.Adler32, "Adler32", 4, () => new Adler32Hash()),
            NonCrypto(HashAlgorithmType.XxHash32, "XXHash32", 4, () => new XxHash32Hash()),
            NonCrypto(HashAlgorithmType.XxHash64, "XXHash64", 8, () => new XxHash64Hash()),
            NonCrypto(HashAlgorithmType.Murmur3_32, "Murmur3-32", 4, () => new Murmur3Hash32()),
            NonCrypto(HashAlgorithmType.Murmur3_128, "Murmur3-128", 16, () => new Murmur3Hash128())
        };

        return defs.ToDictionary(d => d.Type, d => d);
    }

    private static HashAlgorithmDefinition Crypto(HashAlgorithmType type, string name, int hashSize, Func<IStreamingHash?> factory)
        => new(type, name, hashSize, true, factory);

    private static HashAlgorithmDefinition NonCrypto(HashAlgorithmType type, string name, int hashSize, Func<IStreamingHash?> factory)
        => new(type, name, hashSize, false, factory);

    private static IStreamingHash? TryCreateByName(string name)
    {
        try
        {
#pragma warning disable SYSLIB0045
            var algorithm = HashAlgorithm.Create(name);
#pragma warning restore SYSLIB0045
            return algorithm is null ? null : new HashAlgorithmWrapper(algorithm);
        }
        catch
        {
            return null;
        }
    }

    private sealed record HashAlgorithmDefinition(
        HashAlgorithmType Type,
        string DisplayName,
        int HashSizeBytes,
        bool IsCryptographic,
        Func<IStreamingHash?> Factory);
}
