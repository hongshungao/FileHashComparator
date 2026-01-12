using FileHashComparator.App.Models;
using FileHashComparator.App.Services.Hashing;
using System.IO;

namespace FileHashComparator.App.Services;

public sealed class HashService
{
    private readonly HashAlgorithmRegistry _registry;

    public HashService(HashAlgorithmRegistry registry)
    {
        _registry = registry;
    }

    public async Task<HashResult> ComputeFileHashAsync(string filePath, HashAlgorithmType algorithmType, CancellationToken cancellationToken)
    {
        if (!_registry.TryCreate(algorithmType, out var algorithm) || algorithm is null)
        {
            return new HashResult(false, null, "Algorithm not supported");
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var buffer = new byte[1024 * 64];
            int read;

            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                algorithm.AppendData(buffer.AsSpan(0, read));
            }

            var hashBytes = algorithm.GetHashAndReset();
            var hashHex = HashFormatting.ToHex(hashBytes);
            return new HashResult(true, hashHex, null);
        }
        catch (OperationCanceledException)
        {
            return new HashResult(false, null, "Canceled");
        }
        catch (Exception ex)
        {
            return new HashResult(false, null, ex.Message);
        }
        finally
        {
            algorithm.Dispose();
        }
    }
}
