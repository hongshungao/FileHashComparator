using CommunityToolkit.Mvvm.ComponentModel;
using FileHashComparator.App.Models;
using FileHashComparator.App.Services;

namespace FileHashComparator.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LocalizationService _localizationService;

    public MainViewModel(
        LocalizationService localizationService,
        HashAlgorithmRegistry registry,
        HashService hashService,
        HashListParser hashListParser,
        IFileDialogService dialogService)
    {
        _localizationService = localizationService;
        Languages = new List<LanguageOption>(_localizationService.SupportedLanguages);

        SingleCompare = new SingleCompareViewModel(localizationService, registry, hashService, dialogService);
        BatchCompare = new BatchCompareViewModel(localizationService, registry, hashService, hashListParser, dialogService);

        SelectedLanguage = Languages.FirstOrDefault(l => l.CultureName.Equals(_localizationService.CurrentCultureName, StringComparison.OrdinalIgnoreCase))
                           ?? Languages.FirstOrDefault();
    }

    public IReadOnlyList<LanguageOption> Languages { get; }

    [ObservableProperty]
    private LanguageOption? selectedLanguage;

    public SingleCompareViewModel SingleCompare { get; }
    public BatchCompareViewModel BatchCompare { get; }

    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value is null)
        {
            return;
        }

        _localizationService.ApplyCulture(value.CultureName, persist: true);
        SingleCompare.RefreshModes();
        BatchCompare.RefreshModes();
    }
}
