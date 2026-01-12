using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileHashComparator.App.Models;
using FileHashComparator.App.Services;
using System.IO;

namespace FileHashComparator.App.ViewModels;

public partial class SingleCompareViewModel : ObservableObject
{
    private readonly LocalizationService _localizationService;
    private readonly HashAlgorithmRegistry _registry;
    private readonly HashService _hashService;
    private readonly IFileDialogService _dialogService;
    private CancellationTokenSource? _cts;

    public SingleCompareViewModel(
        LocalizationService localizationService,
        HashAlgorithmRegistry registry,
        HashService hashService,
        IFileDialogService dialogService)
    {
        _localizationService = localizationService;
        _registry = registry;
        _hashService = hashService;
        _dialogService = dialogService;

        Algorithms = new List<HashAlgorithmInfo>(_registry.Algorithms.Where(a => a.IsSupported));

        ComputeCommand = new AsyncRelayCommand(ComputeAsync, CanComputeInternal);
        CancelCommand = new RelayCommand(Cancel, () => IsBusy);
        ClearCommand = new RelayCommand(Clear);
        BrowseFileCommand = new RelayCommand(BrowseFile);
        BrowseFileACommand = new RelayCommand(BrowseFileA);
        BrowseFileBCommand = new RelayCommand(BrowseFileB);

        RefreshModes();
        SelectedAlgorithm = Algorithms.FirstOrDefault();
    }

    public IReadOnlyList<HashAlgorithmInfo> Algorithms { get; }

    public IAsyncRelayCommand ComputeCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ClearCommand { get; }
    public IRelayCommand BrowseFileCommand { get; }
    public IRelayCommand BrowseFileACommand { get; }
    public IRelayCommand BrowseFileBCommand { get; }

    [ObservableProperty]
    private ModeOption<SingleCompareMode>? selectedMode;

    [ObservableProperty]
    private HashAlgorithmInfo? selectedAlgorithm;

    [ObservableProperty]
    private string? filePath;

    [ObservableProperty]
    private string? expectedHash;

    [ObservableProperty]
    private string? filePathA;

    [ObservableProperty]
    private string? filePathB;

    [ObservableProperty]
    private string? hashA;

    [ObservableProperty]
    private string? hashB;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isFileVsExpectedMode;

    [ObservableProperty]
    private bool isFileVsFileMode;

    [ObservableProperty]
    private bool canCompute;

    public IReadOnlyList<ModeOption<SingleCompareMode>> Modes { get; private set; } = Array.Empty<ModeOption<SingleCompareMode>>();

    public void RefreshModes()
    {
        var current = SelectedMode?.Mode;
        var modes = new List<ModeOption<SingleCompareMode>>
        {
            new(SingleCompareMode.FileVsExpected, _localizationService.GetString("ModeFileVsExpected")),
            new(SingleCompareMode.FileVsFile, _localizationService.GetString("ModeFileVsFile"))
        };

        Modes = modes;
        OnPropertyChanged(nameof(Modes));

        SelectedMode = modes.FirstOrDefault(m => m.Mode == current) ?? modes.FirstOrDefault();
        UpdateModeFlags();
    }

    partial void OnSelectedModeChanged(ModeOption<SingleCompareMode>? value)
    {
        UpdateModeFlags();
        UpdateCanCompute();
    }

    partial void OnSelectedAlgorithmChanged(HashAlgorithmInfo? value) => UpdateCanCompute();
    partial void OnFilePathChanged(string? value) => UpdateCanCompute();
    partial void OnFilePathAChanged(string? value) => UpdateCanCompute();
    partial void OnFilePathBChanged(string? value) => UpdateCanCompute();
    partial void OnIsBusyChanged(bool value)
    {
        UpdateCanCompute();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateModeFlags()
    {
        IsFileVsExpectedMode = SelectedMode?.Mode == SingleCompareMode.FileVsExpected;
        IsFileVsFileMode = SelectedMode?.Mode == SingleCompareMode.FileVsFile;
    }

    private void UpdateCanCompute()
    {
        CanCompute = CanComputeInternal();
        ComputeCommand.NotifyCanExecuteChanged();
    }

    private bool CanComputeInternal()
    {
        if (IsBusy || SelectedAlgorithm is null)
        {
            return false;
        }

        return SelectedMode?.Mode switch
        {
            SingleCompareMode.FileVsExpected => !string.IsNullOrWhiteSpace(FilePath),
            SingleCompareMode.FileVsFile => !string.IsNullOrWhiteSpace(FilePathA) && !string.IsNullOrWhiteSpace(FilePathB),
            _ => false
        };
    }

    private void BrowseFile()
    {
        var path = _dialogService.PickFile();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePath = path;
        }
    }

    private void BrowseFileA()
    {
        var path = _dialogService.PickFile();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePathA = path;
        }
    }

    private void BrowseFileB()
    {
        var path = _dialogService.PickFile();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePathB = path;
        }
    }

    private async Task ComputeAsync()
    {
        if (!CanComputeInternal())
        {
            return;
        }

        IsBusy = true;
        _cts = new CancellationTokenSource();
        HashA = null;
        HashB = null;
        StatusMessage = null;

        try
        {
            if (SelectedMode?.Mode == SingleCompareMode.FileVsExpected)
            {
                await ComputeFileVsExpectedAsync(_cts.Token);
            }
            else if (SelectedMode?.Mode == SingleCompareMode.FileVsFile)
            {
                await ComputeFileVsFileAsync(_cts.Token);
            }
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task ComputeFileVsExpectedAsync(CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(FilePath) || SelectedAlgorithm is null)
        {
            return;
        }

        if (!File.Exists(FilePath))
        {
            StatusMessage = _localizationService.GetString("StatusMissing");
            return;
        }

        var result = await _hashService.ComputeFileHashAsync(FilePath, SelectedAlgorithm.Type, token);
        if (!result.Success)
        {
            StatusMessage = _localizationService.GetString("StatusError") + ": " + result.Error;
            return;
        }

        HashA = result.HashHex;

        if (string.IsNullOrWhiteSpace(ExpectedHash))
        {
            StatusMessage = _localizationService.GetString("StatusComputed");
            return;
        }

        if (!HashFormatting.TryNormalizeHex(ExpectedHash, out var normalized))
        {
            StatusMessage = _localizationService.GetString("StatusInvalidExpected");
            return;
        }

        if (SelectedAlgorithm.HashSizeBytes > 0 && normalized.Length != SelectedAlgorithm.HashSizeBytes * 2)
        {
            StatusMessage = _localizationService.GetString("StatusInvalidExpected");
            return;
        }

        StatusMessage = string.Equals(HashA, normalized, StringComparison.OrdinalIgnoreCase)
            ? _localizationService.GetString("StatusMatch")
            : _localizationService.GetString("StatusMismatch");
    }

    private async Task ComputeFileVsFileAsync(CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(FilePathA) || string.IsNullOrWhiteSpace(FilePathB) || SelectedAlgorithm is null)
        {
            return;
        }

        if (!File.Exists(FilePathA) || !File.Exists(FilePathB))
        {
            StatusMessage = _localizationService.GetString("StatusMissing");
            return;
        }

        var resultA = await _hashService.ComputeFileHashAsync(FilePathA, SelectedAlgorithm.Type, token);
        if (!resultA.Success)
        {
            StatusMessage = _localizationService.GetString("StatusError") + ": " + resultA.Error;
            return;
        }

        var resultB = await _hashService.ComputeFileHashAsync(FilePathB, SelectedAlgorithm.Type, token);
        if (!resultB.Success)
        {
            StatusMessage = _localizationService.GetString("StatusError") + ": " + resultB.Error;
            return;
        }

        HashA = resultA.HashHex;
        HashB = resultB.HashHex;

        StatusMessage = string.Equals(HashA, HashB, StringComparison.OrdinalIgnoreCase)
            ? _localizationService.GetString("StatusMatch")
            : _localizationService.GetString("StatusMismatch");
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void Clear()
    {
        FilePath = null;
        FilePathA = null;
        FilePathB = null;
        ExpectedHash = null;
        HashA = null;
        HashB = null;
        StatusMessage = null;
    }
}
