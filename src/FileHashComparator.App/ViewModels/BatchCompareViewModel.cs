using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileHashComparator.App.Models;
using FileHashComparator.App.Services;
using System.Collections.ObjectModel;
using System.IO;

namespace FileHashComparator.App.ViewModels;

public partial class BatchCompareViewModel : ObservableObject
{
    private readonly LocalizationService _localizationService;
    private readonly HashAlgorithmRegistry _registry;
    private readonly HashService _hashService;
    private readonly HashListParser _parser;
    private readonly IFileDialogService _dialogService;
    private CancellationTokenSource? _cts;

    public BatchCompareViewModel(
        LocalizationService localizationService,
        HashAlgorithmRegistry registry,
        HashService hashService,
        HashListParser parser,
        IFileDialogService dialogService)
    {
        _localizationService = localizationService;
        _registry = registry;
        _hashService = hashService;
        _parser = parser;
        _dialogService = dialogService;

        Algorithms = new List<HashAlgorithmInfo>(_registry.Algorithms.Where(a => a.IsSupported));

        Results = new ObservableCollection<CompareEntry>();

        CompareCommand = new AsyncRelayCommand(CompareAsync, CanCompareInternal);
        CancelCommand = new RelayCommand(Cancel, () => IsBusy);
        ClearCommand = new RelayCommand(Clear);
        BrowseListFileCommand = new RelayCommand(BrowseListFile);
        BrowseListFileBCommand = new RelayCommand(BrowseListFileB);
        BrowseFolderCommand = new RelayCommand(BrowseFolder);

        RefreshModes();
        SelectedAlgorithm = Algorithms.FirstOrDefault();
    }

    public IReadOnlyList<HashAlgorithmInfo> Algorithms { get; }
    public ObservableCollection<CompareEntry> Results { get; }

    public IAsyncRelayCommand CompareCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand BrowseListFileCommand { get; }
    public IRelayCommand BrowseListFileBCommand { get; }
    public IRelayCommand BrowseFolderCommand { get; }

    [ObservableProperty]
    private ModeOption<BatchCompareMode>? selectedMode;

    [ObservableProperty]
    private HashAlgorithmInfo? selectedAlgorithm;

    [ObservableProperty]
    private string? listFilePath;

    [ObservableProperty]
    private string? listFilePathB;

    [ObservableProperty]
    private string? folderPath;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isListVsFolderMode;

    [ObservableProperty]
    private bool isListVsListMode;

    [ObservableProperty]
    private bool canCompare;

    [ObservableProperty]
    private string? statusMessage;

    public IReadOnlyList<ModeOption<BatchCompareMode>> Modes { get; private set; } = Array.Empty<ModeOption<BatchCompareMode>>();

    public void RefreshModes()
    {
        var current = SelectedMode?.Mode;
        var modes = new List<ModeOption<BatchCompareMode>>
        {
            new(BatchCompareMode.ListVsFolder, _localizationService.GetString("ModeListVsFolder")),
            new(BatchCompareMode.ListVsList, _localizationService.GetString("ModeListVsList"))
        };

        Modes = modes;
        OnPropertyChanged(nameof(Modes));

        SelectedMode = modes.FirstOrDefault(m => m.Mode == current) ?? modes.FirstOrDefault();
        UpdateModeFlags();
    }

    partial void OnSelectedModeChanged(ModeOption<BatchCompareMode>? value)
    {
        UpdateModeFlags();
        UpdateCanCompare();
    }

    partial void OnSelectedAlgorithmChanged(HashAlgorithmInfo? value) => UpdateCanCompare();
    partial void OnListFilePathChanged(string? value) => UpdateCanCompare();
    partial void OnListFilePathBChanged(string? value) => UpdateCanCompare();
    partial void OnFolderPathChanged(string? value) => UpdateCanCompare();
    partial void OnIsBusyChanged(bool value)
    {
        UpdateCanCompare();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateModeFlags()
    {
        IsListVsFolderMode = SelectedMode?.Mode == BatchCompareMode.ListVsFolder;
        IsListVsListMode = SelectedMode?.Mode == BatchCompareMode.ListVsList;
    }

    private void UpdateCanCompare()
    {
        CanCompare = CanCompareInternal();
        CompareCommand.NotifyCanExecuteChanged();
    }

    private bool CanCompareInternal()
    {
        if (IsBusy || SelectedAlgorithm is null)
        {
            return false;
        }

        return SelectedMode?.Mode switch
        {
            BatchCompareMode.ListVsFolder => !string.IsNullOrWhiteSpace(ListFilePath) && !string.IsNullOrWhiteSpace(FolderPath),
            BatchCompareMode.ListVsList => !string.IsNullOrWhiteSpace(ListFilePath) && !string.IsNullOrWhiteSpace(ListFilePathB),
            _ => false
        };
    }

    private void BrowseListFile()
    {
        var path = _dialogService.PickFile("Hash list|*.txt;*.md5;*.sha1;*.sha256;*.sha512|All Files|*.*");
        if (!string.IsNullOrWhiteSpace(path))
        {
            ListFilePath = path;
        }
    }

    private void BrowseListFileB()
    {
        var path = _dialogService.PickFile("Hash list|*.txt;*.md5;*.sha1;*.sha256;*.sha512|All Files|*.*");
        if (!string.IsNullOrWhiteSpace(path))
        {
            ListFilePathB = path;
        }
    }

    private void BrowseFolder()
    {
        var path = _dialogService.PickFolder();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FolderPath = path;
        }
    }

    private async Task CompareAsync()
    {
        if (!CanCompareInternal())
        {
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        Results.Clear();
        _cts = new CancellationTokenSource();

        try
        {
            if (SelectedMode?.Mode == BatchCompareMode.ListVsFolder)
            {
                await CompareListVsFolderAsync(_cts.Token);
            }
            else if (SelectedMode?.Mode == BatchCompareMode.ListVsList)
            {
                await CompareListVsListAsync(_cts.Token);
            }
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task CompareListVsFolderAsync(CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(ListFilePath) || string.IsNullOrWhiteSpace(FolderPath) || SelectedAlgorithm is null)
        {
            return;
        }

        if (!File.Exists(ListFilePath) || !Directory.Exists(FolderPath))
        {
            StatusMessage = _localizationService.GetString("StatusMissing");
            return;
        }

        var entries = _parser.Parse(ListFilePath);
        var normalizedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            token.ThrowIfCancellationRequested();

            if (!entry.IsValid || string.IsNullOrWhiteSpace(entry.Path) || string.IsNullOrWhiteSpace(entry.Hash))
            {
                Results.Add(new CompareEntry(
                    $"Line {entry.LineNumber}",
                    null,
                    null,
                    CompareStatus.ParseError,
                    _localizationService.GetString("StatusParseError")));
                continue;
            }

            var normalizedPath = NormalizePath(entry.Path);
            normalizedPaths.Add(normalizedPath);

            if (!HashFormatting.TryNormalizeHex(entry.Hash, out var expected))
            {
                Results.Add(new CompareEntry(
                    entry.Path,
                    entry.Hash,
                    null,
                    CompareStatus.InvalidExpected,
                    _localizationService.GetString("StatusInvalidExpected")));
                continue;
            }

            if (SelectedAlgorithm.HashSizeBytes > 0 && expected.Length != SelectedAlgorithm.HashSizeBytes * 2)
            {
                Results.Add(new CompareEntry(
                    entry.Path,
                    entry.Hash,
                    null,
                    CompareStatus.InvalidExpected,
                    _localizationService.GetString("StatusInvalidExpected")));
                continue;
            }

            var resolvedPath = ResolvePath(FolderPath, entry.Path);
            if (!File.Exists(resolvedPath))
            {
                Results.Add(new CompareEntry(
                    entry.Path,
                    expected,
                    null,
                    CompareStatus.Missing,
                    _localizationService.GetString("StatusMissing")));
                continue;
            }

            var result = await _hashService.ComputeFileHashAsync(resolvedPath, SelectedAlgorithm.Type, token);
            if (!result.Success || result.HashHex is null)
            {
                Results.Add(new CompareEntry(
                    entry.Path,
                    expected,
                    null,
                    CompareStatus.Unreadable,
                    _localizationService.GetString("StatusUnreadable")));
                continue;
            }

            var status = string.Equals(result.HashHex, expected, StringComparison.OrdinalIgnoreCase)
                ? CompareStatus.Match
                : CompareStatus.Mismatch;

            Results.Add(new CompareEntry(
                entry.Path,
                expected,
                result.HashHex,
                status,
                _localizationService.GetString(status == CompareStatus.Match ? "StatusMatch" : "StatusMismatch")));
        }

        foreach (var file in Directory.EnumerateFiles(FolderPath, "*", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(FolderPath, file);
            var normalized = NormalizePath(relative);
            if (normalizedPaths.Contains(normalized))
            {
                continue;
            }

            Results.Add(new CompareEntry(
                relative,
                null,
                null,
                CompareStatus.Extra,
                _localizationService.GetString("StatusExtra")));
        }
    }

    private Task CompareListVsListAsync(CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(ListFilePath) || string.IsNullOrWhiteSpace(ListFilePathB) || SelectedAlgorithm is null)
        {
            return Task.CompletedTask;
        }

        if (!File.Exists(ListFilePath) || !File.Exists(ListFilePathB))
        {
            StatusMessage = _localizationService.GetString("StatusMissing");
            return Task.CompletedTask;
        }

        var leftEntries = BuildEntryMap(_parser.Parse(ListFilePath));
        var rightEntries = BuildEntryMap(_parser.Parse(ListFilePathB));

        var allPaths = new HashSet<string>(leftEntries.Keys, StringComparer.OrdinalIgnoreCase);
        foreach (var path in rightEntries.Keys)
        {
            allPaths.Add(path);
        }

        foreach (var path in allPaths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            token.ThrowIfCancellationRequested();

            leftEntries.TryGetValue(path, out var left);
            rightEntries.TryGetValue(path, out var right);

            if (left is null)
            {
                Results.Add(new CompareEntry(
                    path,
                    null,
                    right?.Hash,
                    CompareStatus.Extra,
                    _localizationService.GetString("StatusExtra")));
                continue;
            }

            if (right is null)
            {
                Results.Add(new CompareEntry(
                    path,
                    left.Hash,
                    null,
                    CompareStatus.Missing,
                    _localizationService.GetString("StatusMissing")));
                continue;
            }

            if (!HashFormatting.TryNormalizeHex(left.Hash, out var leftHash) ||
                !HashFormatting.TryNormalizeHex(right.Hash, out var rightHash))
            {
                Results.Add(new CompareEntry(
                    path,
                    left.Hash,
                    right.Hash,
                    CompareStatus.InvalidExpected,
                    _localizationService.GetString("StatusInvalidExpected")));
                continue;
            }

            if (SelectedAlgorithm.HashSizeBytes > 0)
            {
                var expectedLength = SelectedAlgorithm.HashSizeBytes * 2;
                if (leftHash.Length != expectedLength || rightHash.Length != expectedLength)
                {
                    Results.Add(new CompareEntry(
                        path,
                        leftHash,
                        rightHash,
                        CompareStatus.InvalidExpected,
                        _localizationService.GetString("StatusInvalidExpected")));
                    continue;
                }
            }

            var match = string.Equals(leftHash, rightHash, StringComparison.OrdinalIgnoreCase);
            Results.Add(new CompareEntry(
                path,
                leftHash,
                rightHash,
                match ? CompareStatus.Match : CompareStatus.Mismatch,
                _localizationService.GetString(match ? "StatusMatch" : "StatusMismatch")));
        }

        return Task.CompletedTask;
    }

    private Dictionary<string, HashListEntry> BuildEntryMap(IReadOnlyList<HashListEntry> entries)
    {
        var map = new Dictionary<string, HashListEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (!entry.IsValid || string.IsNullOrWhiteSpace(entry.Path) || string.IsNullOrWhiteSpace(entry.Hash))
            {
                Results.Add(new CompareEntry(
                    $"Line {entry.LineNumber}",
                    null,
                    null,
                    CompareStatus.ParseError,
                    _localizationService.GetString("StatusParseError")));
                continue;
            }

            var normalized = NormalizePath(entry.Path);
            if (map.ContainsKey(normalized))
            {
                Results.Add(new CompareEntry(
                    entry.Path,
                    entry.Hash,
                    null,
                    CompareStatus.ParseError,
                    _localizationService.GetString("StatusParseError")));
                continue;
            }

            map[normalized] = entry;
        }

        return map;
    }

    private static string NormalizePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }
        return normalized;
    }

    private static string ResolvePath(string root, string entryPath)
    {
        var sanitized = entryPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.IsPathRooted(sanitized) ? sanitized : Path.Combine(root, sanitized);
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void Clear()
    {
        ListFilePath = null;
        ListFilePathB = null;
        FolderPath = null;
        StatusMessage = null;
        Results.Clear();
    }
}
