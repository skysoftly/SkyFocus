using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class AppsListViewModel : ViewModelBase
{
    private AppDbService AppDbService { get; }

    [ObservableProperty] private ObservableCollection<AppRowDto> _apps;
    

    [ObservableProperty] private AppRowDto? _selectedApp;
    public event Action<AppRowDto?>? SelectedAppChanged;
    
    partial void OnSelectedAppChanged(AppRowDto? value)
    {
        SelectedAppChanged?.Invoke(value);
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
        var list = await AppDbService.LoadAppsAsync();

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
                app.IsRunning = running.Contains(app.ProcessName);
            }
            ResortApps();
        });
    }
    
    private void OnActiveAppChanged(string processName)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var app in Apps)
            {
                app.IsActive = app.ProcessName == processName;
            }
        });
    }
    
    

    [RelayCommand]
    public async Task ToggleFavorite(AppRowDto app)
    {
        ResortApps();
        await AppDbService.UpdateAppAsync(app);
    }
    
    
    
    public void ResortApps()
    {
        var selectedPath = SelectedApp?.Path;

        var sorted = Apps
            .OrderByDescending(a => a.IsFavorite)
            .ThenByDescending(a => a.IsRunning)
            .ThenBy(a => a.Name)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            var item = sorted[i];
            var currentIndex = Apps.IndexOf(item);

            if (currentIndex == i || currentIndex == -1)
                continue;

            Apps.Move(currentIndex, i);
        }

        if (selectedPath != null)
        {
            SelectedApp = Apps.FirstOrDefault(a => a.Path == selectedPath);
        }
    }

    public async Task Delete(AppRowDto selectedApp)
    {
        Apps.Remove(selectedApp);
        await AppDbService.RemoveAppAsync(selectedApp.Id);
    }
}