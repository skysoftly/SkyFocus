using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Avalonia;
using SkyFocus.DTOs;
using SkyFocus.Services;
using SkyFocus.Utils;

namespace SkyFocus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public TrackingService TrackingService { get; }
    public WindowBarViewModel WindowBar { get; }


    [ObservableProperty] private ViewModelBase _currentPage;
    [ObservableProperty] private SettingsViewModel _settings;

    private MainPageViewModel _mainPage;
    private ChartPageViewModel _chartPage;


    public MainWindowViewModel(MainPageViewModel mainPageViewModel, ChartPageViewModel chartPageViewModel, SettingsViewModel settingsViewModel, TrackingService tracking, WindowBarViewModel windowBar)
    {
        _mainPage = mainPageViewModel;
        _chartPage = chartPageViewModel;
        TrackingService = tracking;
        WindowBar = windowBar;
        Settings = settingsViewModel;
        
        CurrentPage = _mainPage;
    }

    [RelayCommand]
    private void SelectMainPage()
    {
        CurrentPage = _mainPage;
    }

    [RelayCommand]
    private void SelectChartPage()
    {
        CurrentPage = _chartPage;
    }
}