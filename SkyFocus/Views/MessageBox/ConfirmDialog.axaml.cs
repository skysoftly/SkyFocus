using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SkyFocus.Views.MessageBox;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    private void OnYesClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnNoClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}