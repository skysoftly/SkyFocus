using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;
using SkyFocus.Utils;

namespace SkyFocus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AppsListViewModel AppsList { get; }
    public AppInfoViewModel AppInfo { get; }
    public OverlayViewModel Overlay { get; }

    public TrackingService TrackingService { get; }
    public AppDbService AppDbService { get; }

    [ObservableProperty] private bool _isMaximized; 

    public MainWindowViewModel(AppsListViewModel appsList, AppInfoViewModel appInfo, TrackingService tracking, AppDbService appDbService, OverlayViewModel overlay)
    {
        AppsList = appsList;
        AppInfo = appInfo;
        Overlay = overlay;
        
        TrackingService = tracking;
        AppDbService = appDbService;
    }

    public MainWindowViewModel()
    {
        AppDbService = new AppDbService();
        TrackingService = new TrackingService();
        AppsList = new AppsListViewModel(TrackingService, AppDbService);
        Overlay = new OverlayViewModel();
        var chartViewModel = new  ChartViewModel(AppDbService); 
        AppInfo = new AppInfoViewModel(AppsList, AppDbService, chartViewModel, Overlay);
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