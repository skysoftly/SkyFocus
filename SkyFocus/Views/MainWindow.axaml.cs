using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyFocus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}
