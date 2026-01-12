namespace FileHashComparator.App.Services.Hashing;

public interface IStreamingHash : IDisposable
{
    int HashSizeBytes { get; }
    void AppendData(ReadOnlySpan<byte> data);
    byte[] GetHashAndReset();
}
