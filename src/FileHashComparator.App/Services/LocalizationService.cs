using FileHashComparator.App.Models;
using System.Globalization;
using System.Windows;

namespace FileHashComparator.App.Services;

public sealed class LocalizationService
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    public LocalizationService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = _settingsService.Load();
        SupportedLanguages = new List<LanguageOption>
        {
            new LanguageOption("zh-CN", "中文"),
            new LanguageOption("en-US", "English"),
            new LanguageOption("ru-RU", "Русский")
        };
    }

    public IReadOnlyList<LanguageOption> SupportedLanguages { get; }

    public string CurrentCultureName { get; private set; } = "zh-CN";

    public event EventHandler? CultureChanged;

    public void ApplySavedCulture()
    {
        var cultureName = string.IsNullOrWhiteSpace(_settings.UiCulture)
            ? CultureInfo.CurrentUICulture.Name
            : _settings.UiCulture;

        ApplyCulture(cultureName, persist: false);
    }

    public void ApplyCulture(string cultureName, bool persist)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return;
        }

        if (!SupportedLanguages.Any(l => string.Equals(l.CultureName, cultureName, StringComparison.OrdinalIgnoreCase)))
        {
            cultureName = "zh-CN";
        }

        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CurrentCultureName = cultureName;

        var newDictionary = new ResourceDictionary
        {
            Source = new Uri($"Resources/Strings.{cultureName}.xaml", UriKind.Relative)
        };

        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        var existing = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Resources/Strings.", StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            dictionaries.Remove(existing);
        }
        dictionaries.Add(newDictionary);

        if (persist)
        {
            _settings.UiCulture = cultureName;
            _settingsService.Save(_settings);
        }

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string resourceKey)
    {
        if (System.Windows.Application.Current.TryFindResource(resourceKey) is string value)
        {
            return value;
        }

        return resourceKey;
    }
}
