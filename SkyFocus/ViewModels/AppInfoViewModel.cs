using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    [ObservableProperty] private int _secondWeek; 
    [ObservableProperty] private int _secondMonth; 
    
    
    [ObservableProperty] private AppRowDto? _selectedApp; 
    public ChartViewModel Chart {get;}

    public AppsListViewModel AppsListViewModel { get; }
    private readonly AppDbService _appDbService;
    private readonly IConfirmService _confirm;

    public AppInfoViewModel(AppsListViewModel appsList, AppDbService appDbService, ChartViewModel chartViewModel, IConfirmService confirm)
    {
        AppsListViewModel = appsList;
        _appDbService = appDbService;
        Chart = chartViewModel;
        _confirm = confirm;
        // appsList.SelectedAppChanged += SelectedAppChanged;
    }

    private async void SelectedAppChanged(AppRowDto? selectedApp)
    {
        if (SelectedApp == selectedApp || selectedApp == null) return;
        
        SelectedApp = selectedApp;

        
        if (SelectedApp != null)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-7);
            var startOfMonth = today.AddDays(-30);
        
            // Получаем за неделю
            var weekData = await _appDbService.GetStatsForAppByDatesAsync(
                SelectedApp.Id, 
                startOfWeek, 
                today
            );
            SecondWeek = weekData.Sum(x => x.UsageTimeSeconds);
        
            // Получаем за месяц
            var monthData = await _appDbService.GetStatsForAppByDatesAsync(
                SelectedApp.Id, 
                startOfMonth, 
                today
            );
            SecondMonth = monthData.Sum(x => x.UsageTimeSeconds);
            
            _ = Chart.UpdateChart(SelectedApp.Id);
        }
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
                    if (p.CloseMainWindow())
                    {
                        if (!p.WaitForExit(10000))
                        {
                            p.Kill();
                        }
                    }
                    else
                    {
                        p.Kill();
                    }
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
    private async Task ToggleFavorite()
    {
        if (SelectedApp == null) return;
        // await AppsListViewModel.ToggleFavorite(SelectedApp);
    }

    
    [RelayCommand]
    public async Task Delete()
    {
        bool ok = await _confirm.Show("Удалить объект?");

        if (!ok)
            return;

        // удаление

        // AppsListViewModel.Delete(SelectedApp!);
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