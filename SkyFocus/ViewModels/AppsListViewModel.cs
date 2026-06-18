using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class AppsListViewModel : ViewModelBase
{
    public ObservableCollection<AppRowDto> Apps { get; set; }
    

    [ObservableProperty] private AppRowDto? _selectedApp;
    public event Action<AppRowDto?>? SelectedAppChanged;
    
    partial void OnSelectedAppChanged(AppRowDto? value)
    {
        SelectedAppChanged?.Invoke(value);
    }

    public AppsListViewModel(TrackingService trackingService)
    {
        Apps = new ObservableCollection<AppRowDto>()
        {
            new()
            {
                Name = "prismlauncher",
                Path = "C:\\Users\\skyso\\AppData\\Local\\Programs\\PrismLauncher\\prismlauncher.exe",
                ProcessName = "prismlauncher",
                IsFavorite = true,
            },
            new()
            {
                Name = "opera",
                Path = "C:\\Users\\skyso\\AppData\\Local\\Programs\\Opera GX\\opera.exe",
                ProcessName = "opera",
                IsRunning= true,
            },
            new()
            {
                Name = "Aseprite",
                Path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Aseprite\\Aseprite.exe",
                ProcessName = "Aseprite",
            }
        };
        
        
        trackingService.RunningAppsChanged += OnRunningAppsChanged;
        trackingService.ActiveAppChanged += OnActiveAppChanged;
    }
    
    private void OnRunningAppsChanged(HashSet<string> running)
    {
        foreach (var app in Apps)
        {
            app.IsRunning = running.Contains(app.ProcessName);
        }

        ResortApps();
    }
    
    private void OnActiveAppChanged(string processName)
    {
        foreach (var app in Apps)
        {
            app.IsActive = app.ProcessName == processName;
        }
    }
    
    

    [RelayCommand]
    private async Task ToggleFavorite(AppRowDto app)
    {
        ResortApps();
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

}