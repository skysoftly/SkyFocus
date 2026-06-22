using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkyFocus.Services;

public partial class TrackingService : ObservableObject
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    private CancellationTokenSource? _cts;
    
    public event Action<HashSet<string>>? RunningAppsChanged;
    public event Action<string?, string>? ActiveAppChanged;
    
    private string? _lastName;
    
    
    [ObservableProperty]
    private bool _isRunning;

    public async Task StartAsync()
    {
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
            
            if (_lastName != name)
            {
                Console.WriteLine(name);
                ActiveAppChanged?.Invoke(_lastName, name);
                _lastName = name;
            }
            
        }
        catch
        {
            // процесс мог закрыться за долю секунды
        }
    }
    private string CleanName(string name)
    {
        name = name.ToLowerInvariant();

        name = name.Replace("webhelper", "")
            .Replace("cef", "")
            .Replace("renderer", "")
            .Replace("gpu", "")
            .Replace("crashpad", "")
            .Replace("utility", "");

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
                names.Add(p.ProcessName);
            }
            catch
            {
                // некоторые процессы могут быть недоступны
            }
        }

        RunningAppsChanged?.Invoke(names);
    }
}