namespace FileHashComparator.App.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? PickFile(string? filter = null, string? initialDirectory = null)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = string.IsNullOrWhiteSpace(filter) ? "All Files|*.*" : filter,
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickFolder(string? initialDirectory = null)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            InitialDirectory = initialDirectory ?? string.Empty,
            UseDescriptionForTitle = true,
            Description = "Select a folder"
        };

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }
}
