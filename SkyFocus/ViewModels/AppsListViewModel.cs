using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class AppsListViewModel : ViewModelBase
{
    private bool _suppressSelectionEvent = false;
    private AppDbService AppDbService { get; }

    [ObservableProperty] private ObservableCollection<AppRowDto> _apps;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private AppRowDto? _selectedApp;


    private System.Timers.Timer? _updateTimer;

    public event Action<AppRowDto?>? SelectedAppChanged;

    private DateTime _lastSwitchTime = DateTime.Now;
    private DateTime _currentDate = DateTime.Today;


    partial void OnSelectedAppChanged(AppRowDto? value)
    {
        if (!_suppressSelectionEvent)
        {
            SelectedAppChanged?.Invoke(value);
        }
    }

    public AppsListViewModel(TrackingService trackingService, AppDbService appDbService)
    {
        Apps = new();
        AppDbService = appDbService;

        _ = LoadAppsAsync();

        trackingService.RunningAppsChanged += OnRunningAppsChanged;
        trackingService.ActiveAppChanged += OnActiveAppChanged;
    }

    private async Task LoadAppsAsync()
    {
        var list = await AppDbService.LoadAppsWithTodayStatsAsync();


        Apps = new ObservableCollection<AppRowDto>(list);


        var tasks = Apps.Select(async app =>
        {
            var icon = IconService.GetIcon(app.Path);

            if (icon != null)
            {
                app.Icon = icon;
            }
        });

        await Task.WhenAll(tasks);
    }

    private void OnRunningAppsChanged(HashSet<string> running)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var app in Apps)
            {
                var isRunning = running.Contains(app.ProcessName);
                if (isRunning != app.IsRunning)
                {
                    app.IsRunning = isRunning;

                    if (app.IsRunning)
                    {
                        app.LaunchCount += 1;
                        _ = AppDbService.UpdateAppAsync(app);
                    }
                }

            }

            ResortApps();
        });
    }

    private void OnActiveAppChanged(string? lastProcessName, string processName)
    {
        var now = DateTime.Now;
        var duration = now - _lastSwitchTime;
        var secondsToAdd = (int)duration.TotalSeconds;

        Dispatcher.UIThread.Post(() =>
        {
            CheckAndResetDailyInternal();
            if (!string.IsNullOrEmpty(lastProcessName) && duration.TotalSeconds >= 1)
            {
                var app = Apps.FirstOrDefault(a => a.ProcessName == lastProcessName);
                if (app != null)
                {
                    _ = AppDbService.AddUsageTimeAsync(app.Id, secondsToAdd);

                    app.UsageTimeSeconds += secondsToAdd;
                }
            }


            foreach (var app in Apps)
            {
                app.IsActive = app.ProcessName == processName;
            }
        });
        _lastSwitchTime = now;
    }

    private void CheckAndResetDailyInternal()
    {
        var today = DateTime.Today;

        if (_currentDate != today)
        {
            _currentDate = today;

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var app in Apps)
                {
                    app.UsageTimeSeconds = 0;
                }

                ResortApps();
                Console.WriteLine("[DEBUG] New day! Stats reset at midnight.");
            });
        }
    }


    [RelayCommand]
    public async Task ToggleFavorite(AppRowDto app)
    {
        ResortApps();
        await AppDbService.UpdateAppAsync(app);
    }


    public void ResortApps()
    {
        _suppressSelectionEvent = true;
        try
        {
            var sorted = Apps
                .OrderByDescending(a => a.IsFavorite)
                .ThenByDescending(a => a.IsRunning)
                .ThenByDescending(a => a.UsageTimeSeconds)
                .ThenBy(a => a.Name)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var item = sorted[i];
                var currentIndex = Apps.IndexOf(item);
                
                if (currentIndex != i)
                {
                    Apps.Move(currentIndex, i);
                }
            }
        }
        finally
        {
            _suppressSelectionEvent = false;
        }
    }

    public async Task Delete(AppRowDto selectedApp)
    {
        Apps.Remove(selectedApp);
        await AppDbService.RemoveAppAsync(selectedApp.Id);
    }
    
    [RelayCommand]
    public async Task AddApp()
    {
        if (App.MainWindow != null)
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
                            Patterns = new[] { "*.exe", "*.url" }
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
                ProcessName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant(),
            };
        
            await AppDbService.AddAsync(app);
        
            Apps.Add(app);
            ResortApps();

            var icon = IconService.GetIcon(filePath);

            if (icon != null)
                app.Icon = icon;
        }
    }
}