using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SkyFocus.DTOs;
using SkyFocus.Views.MessageBox;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

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

        trackingService.RunningAppsChanged += OnRunningAppsChanged;
        trackingService.ActiveAppChanged += OnActiveAppChanged;
    }

    private async Task LoadApps()
    {
        var list = await _appDbService.LoadAppsWithTodayStatsAsync();


        _apps = new ObservableCollection<AppRowDto>(list);


        foreach (var app in _apps)
        {
            await LoadIconAsync(app);
        }

        Sort();
    }


    private void Sort()
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
        var filePaths = await FilePickerService.PickExeFilesAsync();
        if (filePaths == null || filePaths.Count == 0) return;

        foreach (var filePath in filePaths)
        {
            var existingByPath = await _appDbService.GetByPathAsync(filePath);
            if (existingByPath != null)
            {
                var dialog = new InfoDialog($"{Path.GetFileNameWithoutExtension(filePath)} уже добавлен!");
                await dialog.ShowDialog(App.MainWindow!);

                continue;
            }
        

            var app = new AppRowDto
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Path = filePath,
                ProcessName = TrackingService.CleanName(Path.GetFileNameWithoutExtension(filePath))
            };

            await _appDbService.AddAsync(app);
        
            await LoadIconAsync(app);
        
            _apps.Add(app);
        }
        Sort();
    }

    public async Task ToggleFavorite(AppRowDto app)
    {
        Sort();
        await _appDbService.UpdateAppAsync(app);
    }

    public async Task<bool> Delete(AppRowDto selectedApp)
    {
        var dialog = new ConfirmDialog("Вы уверены?");
        var ok = await dialog.ShowDialog<bool>(App.MainWindow!);

        if (!ok) return false;
        
        _apps.Remove(selectedApp);
        FilteredApps.Remove(selectedApp);
        SelectedApp = null;
        await _appDbService.RemoveAppAsync(selectedApp.Id);
        
        return true;
    }
    
    
    private async Task LoadIconAsync(AppRowDto app)
    {
        try
        {
            // Используем новый сервис
            var icon = IconService.GetIconForApp(app.Id, app.Path);
            if (icon != null)
            {
                app.Icon = icon;
                app.IconPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SkyFocus",
                    "Icons",
                    $"{app.Id}.png"
                );
                await _appDbService.UpdateAppAsync(app);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Icon error for {app.Name}: {ex.Message}");
        }
    }
    
    
    public async Task EditIcon()
    {
        if (SelectedApp == null) return;

        var path = await FilePickerService.PickImageFileAsync();
        if (path == null) return;

        // Устанавливаем кастомную иконку
        var icon = IconService.SetCustomIcon(SelectedApp.Id, path);
        if (icon != null)
        {
            SelectedApp.Icon = icon;
            SelectedApp.IconPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SkyFocus",
                "Icons",
                $"{SelectedApp.Id}.png"
            );
            await _appDbService.UpdateAppAsync(SelectedApp);
        }
    }

    public async Task ResetIcon()
    {
        if (SelectedApp == null) return;

        // Удаляем кастомную иконку
        IconService.DeleteIcon(SelectedApp.Id);
        
        // Загружаем заново из exe
        await LoadIconAsync(SelectedApp);
    }
    
    public async Task<List<AppRowDto>> GetTopAppsAsync(int count = 5)
    {
        var apps = await _appDbService.LoadAppsWithTodayStatsAsync();
        return apps.OrderByDescending(a => a.UsageTimeSeconds).Take(count).ToList();
    }
}