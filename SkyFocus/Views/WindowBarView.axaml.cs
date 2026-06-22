using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public partial class WindowBarView : UserControl
{
    public WindowBarView()
    {
        InitializeComponent();
        
        this.PropertyChanged += OnWindowPropertyChanged;
    }
    
    
    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        App.MainWindow.BeginMoveDrag(e);
    }
    
    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "WindowState")
        {
            if (DataContext is WindowBarViewModel vm)
            {
                vm.IsMaximized = App.MainWindow.WindowState == WindowState.Maximized;
            }
        }
    }
}