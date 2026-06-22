using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
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

    partial void OnSearchTextChanged(string value)
    {
        Sort();
    }

    public AppService(AppDbService appDbService)
    {
        _appDbService = appDbService;

        _ = LoadApps();

        Sort();
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

    [RelayCommand]
    private async Task AddApp()
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
}