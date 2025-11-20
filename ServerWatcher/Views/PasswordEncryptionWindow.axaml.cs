using Avalonia;
using Avalonia.Controls;
using ServerWatcher.ViewModels;

namespace ServerWatcher.Views;

public partial class PasswordEncryptionWindow : Window
{
    public PasswordEncryptionWindow()
    {
        InitializeComponent();
        DataContext = new PasswordEncryptionViewModel();
    }
}
