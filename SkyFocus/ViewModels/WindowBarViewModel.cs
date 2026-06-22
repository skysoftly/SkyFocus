using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkyFocus.ViewModels;

public partial class WindowBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isMaximized;
    
    [RelayCommand]
    private void CloseWindow()
    {
        App.MainWindow.Close();
    }
    
    [RelayCommand]
    private void MinimizeWindow()
    {
        App.MainWindow.WindowState = WindowState.Minimized;
    }
    
    [RelayCommand]
    private void MaxRestoreWindow()
    {
        App.MainWindow.WindowState = App.MainWindow.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        IsMaximized = App.MainWindow.WindowState == WindowState.Maximized;
    }

}