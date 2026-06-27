using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;
using SkyFocus.Views.MessageBox;

namespace SkyFocus.ViewModels;

public partial class AppInfoViewModel : ViewModelBase
{
    private readonly AppDbService _appDbService;
    private readonly AppService _appService;

    [ObservableProperty] private int _secondWeek;
    [ObservableProperty] private int _secondMonth;
    [ObservableProperty] private int _secondAll;

    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private int _caretIndex;

    [ObservableProperty] AppRowDto? _selectedApp;
    public ChartViewModel Chart { get; }


    partial void OnIsEditingChanged(bool value)
    {
        if (!value)
            _ = SaveEdit();
    }

    public AppInfoViewModel(ChartViewModel chartViewModel, AppService appService, AppDbService appDbService)
    {
        Chart = chartViewModel;
        _appService = appService;
        _appDbService = appDbService;

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
            var startOfAll = DateTime.MinValue;

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

            // Получаем за всё время
            var allData = await _appDbService.GetStatsForAppByDatesAsync(
                SelectedApp.Id,
                startOfAll,
                today
            );
            SecondAll = allData.Sum(x => x.UsageTimeSeconds);


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
                        await Task.Run(() => p.WaitForExit(2000));
                        if (!p.HasExited)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
        bool ok = await _appService.Delete(SelectedApp!);
        if (ok)
            SelectedApp = null;
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
            await _appDbService.UpdateAppAsync(SelectedApp);
    }

    [RelayCommand]
    private async Task EditPath()
    {
        if (SelectedApp == null) return;

        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var filePath = await FilePickerService.PickExeFileAsync();
                if (string.IsNullOrEmpty(filePath)) return;

                // Проверка длины пути
                if (filePath.Length >= 250)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var dialog = new InfoDialog("Путь слишком длинный!");
                        await dialog.ShowDialog(App.MainWindow!);
                    });
                    return;
                }

                // Проверка на дубликат (исключаем текущее приложение)
                var existingByPath = await _appDbService.GetByPathAsync(filePath);
                if (existingByPath != null && existingByPath.Id != SelectedApp.Id)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var dialog = new InfoDialog("Это приложение уже добавлено!");
                        await dialog.ShowDialog(App.MainWindow!);
                    });
                    return;
                }

                // Обновляем
                SelectedApp.Path = filePath;
                SelectedApp.ProcessName = TrackingService.CleanName(Path.GetFileNameWithoutExtension(filePath));

                await _appDbService.UpdateAppAsync(SelectedApp);

                // Обновляем иконку
                await _appService.ResetIcon(SelectedApp);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EditPath error: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var infoDialog = new InfoDialog($"Ошибка: {ex.Message}");
                await infoDialog.ShowDialog(App.MainWindow!);
            });
        }
    }

    [RelayCommand]
    private async Task EditName()
    {
        if (SelectedApp == null) return;

        try
        {
            var name = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new TextDialog("Введите новое имя!");

                if (App.MainWindow == null)
                    return (string?)null;

                return await dialog.ShowDialog<string?>(App.MainWindow);
            });


            if (string.IsNullOrEmpty(name)) return;

            SelectedApp.Name = name;
            await _appDbService.UpdateAppAsync(SelectedApp);
        }
        catch (Exception ex)
        {
            // Отменено
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var infoDialog = new InfoDialog($"Ошибка: {ex.Message}");
                await infoDialog.ShowDialog(App.MainWindow!);
            });
        }
    }

    [RelayCommand]
    private async Task EditIcon()
    {
        if (SelectedApp == null) return;
        await Dispatcher.UIThread.InvokeAsync(async () => { await _appService.EditIcon(); });
    }

    [RelayCommand]
    private async Task ResetIcon()
    {
        if (SelectedApp == null) return;
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _appService.ResetIcon(SelectedApp); 
        });
    }
}