using FileHashComparator.App.Services;
using FileHashComparator.App.ViewModels;
using System.Windows;

namespace FileHashComparator.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = new SettingsService();
        var localizationService = new LocalizationService(settingsService);
        localizationService.ApplySavedCulture();

        var registry = new HashAlgorithmRegistry();
        var hashService = new HashService(registry);
        var parser = new HashListParser();
        var dialogService = new FileDialogService();

        var mainViewModel = new MainViewModel(localizationService, registry, hashService, parser, dialogService);
        var window = new MainWindow
        {
            DataContext = mainViewModel
        };

        window.Show();
    }
}
