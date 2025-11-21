using Avalonia.Controls;
using ServerWatcher.ViewModels;

namespace ServerWatcher.Views;

public partial class FileBrowserWindow : Window
{
    public FileBrowserWindow()
    {
        InitializeComponent();

        if (DataContext is FileBrowserViewModel viewModel)
        {
            viewModel.SetWindow(this);
        }
    }
}
