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

    private MainPageViewModel _mainPage;
    private ChartPageViewModel _chartPage;
    private SettingsPageViewModel _settingsPage;


    public MainWindowViewModel(MainPageViewModel mainPageViewModel, ChartPageViewModel chartPageViewModel, SettingsPageViewModel settingsPageViewModel, TrackingService tracking, WindowBarViewModel windowBar)
    {
        _mainPage = mainPageViewModel;
        _chartPage = chartPageViewModel;
        _settingsPage = settingsPageViewModel;
        TrackingService = tracking;
        WindowBar = windowBar;
        
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
        _ = _chartPage.UpdateChart();
        CurrentPage = _chartPage;
    }
    
    [RelayCommand]
    private void SelectSettingsPage()
    {
        _ = _settingsPage.Update();
        CurrentPage = _settingsPage;
    }
}