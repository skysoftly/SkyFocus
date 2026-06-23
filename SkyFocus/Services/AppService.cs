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

namespace SkyFocus.Services;

public partial class AppService : ObservableObject
{
    private readonly AppDbService _appDbService;

    private ObservableCollection<AppRowDto> _apps = new();
    [ObservableProperty] private ObservableCollection<AppRowDto> _filteredApps = new();
    [ObservableProperty] private AppRowDto? _selectedApp;

    [ObservableProperty] private string _searchText = string.Empty;

    
    private DateTime _lastSwitchTime = DateTime.Now;
    private DateTime _currentDate = DateTime.Today;
    
    partial void OnSearchTextChanged(string value)
    {
        Sort();
    }
    
    public AppService(AppDbService appDbService, TrackingService trackingService)
    {
        _appDbService = appDbService;

        _ = LoadApps();

        Sort();
        
        trackingService.RunningAppsChanged += OnRunningAppsChanged;
        trackingService.ActiveAppChanged += OnActiveAppChanged;
    }

    private async Task LoadApps()
    {
        var list = await _appDbService.LoadAppsWithTodayStatsAsync();


        _apps = new ObservableCollection<AppRowDto>(list);


        var tasks = _apps.Select(app =>
        {
            try
            {
                var icon = IconService.GetIcon(app.Path);

                if (icon != null)
                {
                    app.Icon = icon;
                }

                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        await Task.WhenAll(tasks);
    }
    
    public void Sort()
    {
        var query = _apps.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(app =>
                app.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        var sorted = query
            .OrderByDescending(a => a.IsFavorite)
            .ThenByDescending(a => a.IsRunning)
            .ThenByDescending(a => a.UsageTimeSeconds)
            .ThenBy(a => a.Name)
            .ToList();

        FilteredApps = new ObservableCollection<AppRowDto>(sorted);
    }

    
    private void OnRunningAppsChanged(HashSet<string> running)
    {
        Dispatcher.UIThread.Post(() =>
        {
            bool isUpdate = false;
            foreach (var app in _apps)
            {
                var isRunning = running.Contains(app.ProcessName);
                if (isRunning != app.IsRunning)
                {
                    app.IsRunning = isRunning;
    
                    if (app.IsRunning)
                    {
                        app.LaunchCount += 1;
                        _ = _appDbService.UpdateAppAsync(app);
                    }
    
                    isUpdate = true;
                }
    
            }
    
            if (isUpdate)
                Sort();
        });
    }
    
    private void OnActiveAppChanged(string? lastProcessName, string processName)
    {
        var now = DateTime.Now;
        var duration = now - _lastSwitchTime;
        var secondsToAdd = (int)duration.TotalSeconds;
        var today = DateTime.Today;
    
        Dispatcher.UIThread.Post(() =>
        {
            if (_currentDate != today)
            {
                _currentDate = today;
                foreach (var app in _apps)
                {
                    app.UsageTimeSeconds = 0;
                }
            }
            
            if (!string.IsNullOrEmpty(lastProcessName) && duration.TotalSeconds >= 1)
            {
                var app = _apps.FirstOrDefault(a => a.ProcessName == lastProcessName);
                if (app != null)
                {
                    _ = _appDbService.AddUsageTimeAsync(app.Id, secondsToAdd);
    
                    app.UsageTimeSeconds += secondsToAdd;
                    Sort();
                }
            }
    
    
            foreach (var app in _apps)
            {
                app.IsActive = app.ProcessName == processName;
            }
        });
        _lastSwitchTime = now;
    }
    
    
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

        await _appDbService.AddAsync(app);

        _apps.Add(app);
        Sort();

        var icon = IconService.GetIcon(filePath);

        if (icon != null)
            app.Icon = icon;
    }

    public async Task ToggleFavorite(AppRowDto app)
    {
        Sort();
        await _appDbService.UpdateAppAsync(app);
    }

    public async Task Delete(AppRowDto selectedApp)
    {
        _apps.Remove(selectedApp);
        await _appDbService.RemoveAppAsync(selectedApp.Id);
    }
}

