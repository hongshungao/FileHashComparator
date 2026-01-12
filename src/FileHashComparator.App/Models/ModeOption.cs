namespace FileHashComparator.App.Models;

public sealed class ModeOption<T>
    where T : struct
{
    public ModeOption(T mode, string displayName)
    {
        Mode = mode;
        DisplayName = displayName;
    }

    public T Mode { get; }
    public string DisplayName { get; }
}
