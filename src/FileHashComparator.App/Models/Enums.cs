namespace FileHashComparator.App.Models;

public enum HashAlgorithmType
{
    Md2,
    Md4,
    Md5,
    Sha1,
    Sha224,
    Sha256,
    Sha384,
    Sha512,
    Sha512_224,
    Sha512_256,
    Sha3_224,
    Sha3_256,
    Sha3_384,
    Sha3_512,
    Shake128,
    Shake256,
    RipeMd160,
    Whirlpool,
    Blake2b,
    Blake2s,
    Blake3,
    Crc32,
    Crc32C,
    Crc64Ecma,
    Adler32,
    XxHash32,
    XxHash64,
    Murmur3_32,
    Murmur3_128
}

public enum SingleCompareMode
{
    FileVsExpected,
    FileVsFile
}

public enum BatchCompareMode
{
    ListVsFolder,
    ListVsList
}

public enum CompareStatus
{
    Match,
    Mismatch,
    Missing,
    Extra,
    Unreadable,
    ParseError,
    InvalidExpected,
    Error,
    Skipped,
    Computed
}
