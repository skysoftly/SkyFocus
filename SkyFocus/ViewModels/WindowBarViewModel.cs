using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.Services;
using SkyFocus.Views;

namespace SkyFocus.ViewModels;

public partial class WindowBarViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isMaximized;

    public WindowState WindowState
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsMaximized = value == WindowState.Maximized;
            }
        }
    }


    [RelayCommand]
    private void CloseWindow()
    {
        App.MainWindow?.Close();
    }

    [RelayCommand]
    private void MinimizeWindow()
    {
        App.MainWindow?.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void MaxRestoreWindow()
    {
        App.MainWindow?.WindowState = App.MainWindow.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        IsMaximized = App.MainWindow?.WindowState == WindowState.Maximized;
    }
}