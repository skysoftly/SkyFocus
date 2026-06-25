using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SkyFocus.Views.MessageBox;

public partial class TextDialog : Window
{
    public TextDialog()
    {
        InitializeComponent();
    }
    
    public TextDialog(string title)
    {
        InitializeComponent();
        
        Title.Text = title;
    }
    
    private void OnYesClick(object? sender, RoutedEventArgs e)
    {
        Close(TextBox.Text);
    }
    private void OnNoClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}