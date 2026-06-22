using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class AppInfoViewModel : ViewModelBase
{
    [ObservableProperty] private AppRowDto? _selectedApp;

    public AppsListViewModel AppsListViewModel { get; }
    private readonly IConfirmService _confirm;

    public AppInfoViewModel(AppsListViewModel appsList, IConfirmService confirm)
    {
        AppsListViewModel = appsList;
        _confirm = confirm;
        appsList.SelectedAppChanged += selectedApp => { SelectedApp = selectedApp; };
    }

    public AppInfoViewModel()
    {
        SelectedApp = new AppRowDto
        {
            Name = "opera",
            Path = @"C:\Users\skyso\AppData\Local\Programs\Opera GX\opera.exe",
            ProcessName = "chrome",
            IsFavorite = true,
            IsRunning = true,
            IsActive = false,
            Icon = IconService.GetIcon(@"C:\Users\skyso\AppData\Local\Programs\Opera GX\opera.exe")
        };
    }

    [RelayCommand]
    private async Task ToggleApp()
    {
        if (SelectedApp == null) return;
        
        if (SelectedApp.IsRunning)
        {
            var processes = Process.GetProcessesByName(SelectedApp.ProcessName);

            foreach (var p in processes)
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                    // нет прав или процесс уже закрылся
                }
            }
        }
        else
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedApp.Path,
                    UseShellExecute = true
                });
            }
            catch
            {
                // не получилось открыть
            }
        }
    }


    [RelayCommand]
    private async Task ToggleFavoriteCommand()
    {
        if (SelectedApp == null) return;
        await AppsListViewModel.ToggleFavorite(SelectedApp);
    }

    
    [RelayCommand]
    public async Task Delete()
    {
        bool ok = await _confirm.Show("Удалить объект?");

        if (!ok)
            return;

        // удаление

        AppsListViewModel.Delete(SelectedApp!);
    }



    [RelayCommand]
    public void OpenFolderCommand()
    {
        
        if (string.IsNullOrEmpty(SelectedApp?.Path)) return;
    
        try
        {
            if (File.Exists(SelectedApp?.Path))
            {
                // Открывает папку и выделяет файл
                Process.Start("explorer.exe", $"/select, \"{SelectedApp?.Path}\"");
            }
            else
            {
                string? folder = Path.GetDirectoryName(SelectedApp?.Path);
                if (!string.IsNullOrEmpty(folder))
                    Process.Start("explorer.exe", folder);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }


    [RelayCommand]
    public async Task CopyPathCommand()
    {
        
        if (string.IsNullOrEmpty(SelectedApp?.Path)) return;
    
        try
        {
            var clipboard = TopLevel.GetTopLevel(App.MainWindow)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(SelectedApp?.Path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка копирования: {ex.Message}");
        }
    }

}