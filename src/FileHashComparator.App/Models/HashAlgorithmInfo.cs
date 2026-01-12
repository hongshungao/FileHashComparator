namespace FileHashComparator.App.Models;

public sealed class HashAlgorithmInfo
{
    public HashAlgorithmInfo(HashAlgorithmType type, string displayName, int hashSizeBytes, bool isSupported, bool isCryptographic)
    {
        Type = type;
        DisplayName = displayName;
        HashSizeBytes = hashSizeBytes;
        IsSupported = isSupported;
        IsCryptographic = isCryptographic;
    }

    public HashAlgorithmType Type { get; }
    public string DisplayName { get; }
    public int HashSizeBytes { get; }
    public bool IsSupported { get; }
    public bool IsCryptographic { get; }
}
