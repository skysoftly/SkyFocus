using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SkyFocus.Services;

public class TrackingService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    private CancellationTokenSource? _cts;
    
    public event Action<HashSet<string>>? RunningAppsChanged;
    public event Action<string>? ActiveAppChanged;

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
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
            var name = process.ProcessName;

            ActiveAppChanged?.Invoke(name);
        }
        catch
        {
            // процесс мог закрыться за долю секунды
        }
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