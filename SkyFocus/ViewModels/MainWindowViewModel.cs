using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;
using SkyFocus.Utils;

namespace SkyFocus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AppsListViewModel AppsList { get; }
    public AppInfoViewModel AppInfo { get; }

    public TrackingService Tracking { get; }

    public MainWindowViewModel(AppsListViewModel appsList, AppInfoViewModel appInfo, TrackingService tracking)
    {
        Tracking = tracking;
        AppsList = appsList;
        AppInfo = appInfo;
    }
    
    
    [RelayCommand]
    public async Task AddApp()
    {
        var files = await App.MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Выберите приложение",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Исполняемые файлы")
                    {
                        Patterns = new[] { "*.exe" }
                    }
                }
            });

        if (files.Count == 0)
            return;

        var filePath = files[0].Path.LocalPath;
        
        
        var app = new AppRowDto
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            Path = filePath,
            ProcessName = Path.GetFileNameWithoutExtension(filePath),
        };
        
        AppsList.Apps.Add(app);

        var icon = await IconService.GetIconAsync(filePath);

        if (icon != null)
            app.Icon = icon;
    }
}