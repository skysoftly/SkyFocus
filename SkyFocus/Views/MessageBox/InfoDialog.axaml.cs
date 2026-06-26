using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SkyFocus.Views.MessageBox;

public partial class InfoDialog : Window
{
    public InfoDialog()
    {
        InitializeComponent();
    }
    public InfoDialog(string title)
    {
        InitializeComponent();
        
        Title.Text = title;
    }
    
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        await Task.Delay(1);
    }
    
    private void OnYesClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}