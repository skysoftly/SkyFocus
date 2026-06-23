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
    private readonly AppDbService _appDbService;
    private readonly AppService _appService;
    private readonly IConfirmService _confirm;
    
    [ObservableProperty] private int _secondWeek; 
    [ObservableProperty] private int _secondMonth; 
    
    [ObservableProperty] private bool _isEditing; 
    [ObservableProperty] private int _caretIndex; 
    
    [ObservableProperty] AppRowDto? _selectedApp;
    public ChartViewModel Chart {get;}


    partial void OnIsEditingChanged(bool value)
    {
        if (!value)
            _ = SaveEdit();
    }

    public AppInfoViewModel(ChartViewModel chartViewModel, AppService appService, AppDbService appDbService, IConfirmService confirmService)
    {
        Chart = chartViewModel;
        _appService = appService;
        _appDbService = appDbService;
        _confirm = confirmService;
        
        _appService.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(_appService.SelectedApp))
            {
                await SelectedAppChanged(_appService.SelectedApp);
            }
        };
        
        SelectedApp = _appService.SelectedApp;
    }


    private async Task SelectedAppChanged(AppRowDto? selectedApp)
    {
        if (SelectedApp == selectedApp || selectedApp == null) return;
        
        SelectedApp = selectedApp;
    
        
        if (SelectedApp != null)
        {
            IsEditing = false;
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
    private void ToggleApp()
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
        await _appService.ToggleFavorite(SelectedApp);
    }

    
    [RelayCommand]
    private async Task Delete()
    {
        bool ok = await _confirm.Show("Удалить объект?");
    
        if (!ok)
            return;
    
        await _appService.Delete(SelectedApp!);
    }



    [RelayCommand]
    private void OpenFolder()
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
    private async Task CopyPath()
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
    
    
    [RelayCommand]
    private void Edit()
    {
        IsEditing = true;
        CaretIndex = SelectedApp?.NoteText.Length ?? 0;
    }

    [RelayCommand]
    private async Task SaveEdit()
    {
        IsEditing = false;

        if (SelectedApp != null) 
            await _appDbService.UpdateAppAsync(SelectedApp );
    }

}