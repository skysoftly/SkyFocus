using System;
using System.IO;

namespace SkyFocus.Services;

public static class DbPath
{
    public static string GetPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SkyFocus");
        
        Directory.CreateDirectory(folder);
        
        return Path.Combine(folder, "skyfocus.db");
    }
}