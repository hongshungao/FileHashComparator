namespace FileHashComparator.App.Services;

public interface IFileDialogService
{
    string? PickFile(string? filter = null, string? initialDirectory = null);
    string? PickFolder(string? initialDirectory = null);
}
