using FileHashComparator.App.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileHashComparator.App.Views;

public partial class BatchCompareView : System.Windows.Controls.UserControl
{
    public BatchCompareView()
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

    private void OnListFileDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not BatchCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && File.Exists(path))
        {
            viewModel.ListFilePath = path;
        }
    }

    private void OnListFileBDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not BatchCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && File.Exists(path))
        {
            viewModel.ListFilePathB = path;
        }
    }

    private void OnFolderDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not BatchCompareViewModel viewModel)
        {
            return;
        }

        if (TryGetFirstPath(e, out var path) && Directory.Exists(path))
        {
            viewModel.FolderPath = path;
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
