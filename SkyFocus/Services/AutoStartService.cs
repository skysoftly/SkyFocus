using System;
using Microsoft.Win32;

namespace SkyFocus.Services;

public class AutoStartService
{
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SkyFocus";
    
    public static void Enable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            if (key != null)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\" --minimized");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AutoStart Enable error: {ex.Message}");
        }
    }
    
    public static void Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            if (key != null)
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AutoStart Disable error: {ex.Message}");
        }
    }
    
    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
            if (key != null)
            {
                var value = key.GetValue(AppName);
                return value != null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AutoStart IsEnabled error: {ex.Message}");
        }
        return false;
    }
}