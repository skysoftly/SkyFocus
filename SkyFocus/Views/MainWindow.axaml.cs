using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.PropertyChanged += OnWindowPropertyChanged;
    }
    
    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
    
    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "WindowState")
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsMaximized = WindowState == WindowState.Maximized;
            }
        }
    }
}
