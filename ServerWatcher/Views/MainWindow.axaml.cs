using Avalonia.Controls;
using Avalonia.Interactivity;
using ServerWatcher.Models;
using ServerWatcher.ViewModels;

namespace ServerWatcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ServerName_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock textBlock && textBlock.DataContext is ServerStatus serverStatus)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.OpenFileBrowserCommand.ExecuteAsync(serverStatus);
            }
        }
    }
}