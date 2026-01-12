using FileHashComparator.App.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileHashComparator.App.Views;

public partial class SingleCompareView : System.Windows.Controls.UserControl
{
    public SingleCompareView()
    {
        InitializeComponent();
    }

    private void OnPreviewDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void OnFilePathDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not SingleCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && File.Exists(path))
        {
            viewModel.FilePath = path;
        }
    }

    private void OnFilePathADrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not SingleCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && File.Exists(path))
        {
            viewModel.FilePathA = path;
        }
    }

    private void OnFilePathBDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not SingleCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && File.Exists(path))
        {
            viewModel.FilePathB = path;
        }
    }

    private static bool TryGetFirstPath(System.Windows.DragEventArgs e, out string path)
    {
        path = string.Empty;
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files || files.Length == 0)
        {
            return false;
        }

        path = files[0];
        return true;
    }
}
