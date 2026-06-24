using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SkyFocus.Views.MessageBox;

public partial class InfoDialog : Window
{
    public InfoDialog(string title)
    {
        InitializeComponent();
        
        Title.Text = title;
    }
    
    private void OnYesClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}