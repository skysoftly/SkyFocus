using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using SkyFocus.Data;
using SkyFocus.Data.Entities;
using SkyFocus.Views.MessageBox;

namespace SkyFocus.Services;

public partial class TrackingService : ObservableObject
{
    private readonly SettingsService _settingsService;
    private HashSet<string> _trackApps;

    private static readonly HashSet<string> _systemProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "explorer",
        "searchapp",
        "searchhost",
        "taskhostw",
        "svchost",
        "winlogon",
        "csrss",
        "lsass",
        "services",
        "system",
        "dwm",
        "conhost",
        "cmd",
        "powershell",
        "runtimebroker",
        "applicationframehost",
        "shellexperiencehost",
        "startmenuexperiencehost",
        "systemsettings",
        "taskmgr",
        "procexp",
        "procexp64",
        "screenclippinghost",
        "openwith"
    };

    public event Action<string>? AppAddedToTracking;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private CancellationTokenSource? _cts;

    public event Action<HashSet<string>>? RunningAppsChanged;
    public event Action<string?, string>? ActiveAppChanged;

    private string? _lastName;

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public TrackingService(IDbContextFactory<AppDbContext> dbFactory, SettingsService settingsService)
    {
        _settingsService = settingsService;
        _dbFactory = dbFactory;
    }


    [ObservableProperty] private bool _isRunning;

    public async Task StartAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        _trackApps = (await db.Tracks
                .Select(x => x.ProcessName)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _cts = new CancellationTokenSource();
        IsRunning = true;
        while (!_cts.Token.IsCancellationRequested)
        {
            CheckRunningApps();
            CheckFocusedWindow();

            await Task.Delay(1000, _cts.Token);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsRunning = false;
    }

    private void CheckFocusedWindow()
    {
        var hwnd = GetForegroundWindow();

        if (hwnd == IntPtr.Zero)
            return;

        GetWindowThreadProcessId(hwnd, out uint pid);

        try
        {
            var process = Process.GetProcessById((int)pid);

            var name = CleanName(process.ProcessName);
            var path = process.MainModule?.FileName;

            if (_lastName != name)
            {
                Console.WriteLine($"Process: {name}");
                ActiveAppChanged?.Invoke(_lastName, name);

                if (!name.Contains("skyfocus") && !_trackApps.Contains(name) && !_systemProcesses.Contains(name) &&
                    _settingsService.Get("IsTrackingNotify", false))
                {
                    _trackApps.Add(name);

                    _ = SuggestAddAppAsync(name, path);
                }

                _lastName = name;
            }
        }
        catch
        {
            // процесс мог закрыться за долю секунды
        }
    }


    private async Task SuggestAddAppAsync(string processName, string? path)
    {
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var exists = await db.Tracks.AnyAsync(t => t.ProcessName == processName);
            if (exists) return;

            var track = new TrackEntity
            {
                ProcessName = processName
            };

            db.Tracks.Add(track);
            await db.SaveChangesAsync();

            var exists2 = await db.Apps.AnyAsync(t => t.ProcessName == processName);
            if (exists2) return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SuggestAddAppAsync check error: {ex.Message}");
            return;
        }

        var displayName = string.IsNullOrEmpty(processName) ? "приложение" : processName;

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                var dialog = new ConfirmDialog($"Добавить \"{displayName}\" в список отслеживаемых?")
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Topmost = true
                };

                var result = await dialog.ShowDialog<bool>(App.MainWindow!);

                if (result)
                {
                    Console.WriteLine($"User confirmed: {processName}");
                    AppAddedToTracking?.Invoke(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dialog error: {ex.Message}");
            }
        }, DispatcherPriority.Normal);

        // // Пользователь согласился — добавляем в AppService (Apps)
        // try
        // {
        //     using var scope = _serviceProvider.CreateScope();
        //     var appService = scope.ServiceProvider.GetRequiredService<AppService>();
        //
        //     // Проверяем, есть ли уже в Apps
        //     var existingApp = await appService.GetAppByPathAsync(path ?? string.Empty);
        //     if (existingApp == null && !string.IsNullOrEmpty(path))
        //     {
        //         var createDto = new CreateAppDto
        //         {
        //             Name = Path.GetFileNameWithoutExtension(path),
        //             Path = path,
        //             ProcessName = processName
        //         };
        //
        //         await appService.AddAppAsync(createDto);
        //         Console.WriteLine($"App added to Apps: {processName}");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"SuggestAddAppAsync (AppService) error: {ex.Message}");
        // }
    }

    public static string CleanName(string name)
    {
        name = name.ToLowerInvariant();

        name = name.Replace("webhelper", "")
            .Replace("cef", "")
            .Replace("renderer", "")
            .Replace("gpu", "")
            .Replace("crashpad", "")
            .Replace("utility", "")
            .Replace(" ", "")
            .Replace(".", "");

        if (string.IsNullOrWhiteSpace(name))
            return "";

        return name;
    }

    private void CheckRunningApps()
    {
        var processes = Process.GetProcesses();

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in processes)
        {
            try
            {
                names.Add(CleanName(p.ProcessName));
            }
            catch
            {
                // некоторые процессы могут быть недоступны
            }
        }

        RunningAppsChanged?.Invoke(names);
    }
}