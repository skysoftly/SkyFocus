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

public partial class MainWindowViewModel(
    AppsListViewModel appsList,
    AppInfoViewModel appInfo,
    TrackingService tracking,
    OverlayViewModel overlay,
    WindowBarViewModel windowBar)
    : ViewModelBase
{
    public WindowBarViewModel WindowBar { get; } = windowBar;
    public AppsListViewModel AppsList { get; } = appsList;
    public AppInfoViewModel AppInfo { get; } = appInfo;
    public OverlayViewModel Overlay { get; } = overlay;

    public TrackingService TrackingService { get; } = tracking;
}