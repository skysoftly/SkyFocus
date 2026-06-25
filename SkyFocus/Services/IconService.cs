using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace SkyFocus.Services;

public class IconService
{
    private static readonly string IconsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SkyFocus",
        "Icons"
    );

    static IconService()
    {
        Directory.CreateDirectory(IconsDir);
    }

    public static Bitmap? GetIconForApp(int appId, string appPath)
    {
        // 1. Проверяем в кэше
        var cachedPath = Path.Combine(IconsDir, $"{appId}.png");
        if (File.Exists(cachedPath))
        {
            try
            {
                return new Bitmap(cachedPath);
            }
            catch
            {
                // Если файл битый - удаляем и пересоздаем
                File.Delete(cachedPath);
            }
        }

        // 2. Извлекаем из exe
        var icon = ExtractFromExe(appPath);
        if (icon != null)
        {
            SaveIcon(appId, icon);
            return icon;
        }

        return null;
    }

    public static Bitmap? SetCustomIcon(int appId, string imagePath)
    {
        try
        {
            var bitmap = new Bitmap(imagePath);
            SaveIcon(appId, bitmap);
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap? ExtractFromExe(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(path);
            if (icon == null) return null;

            using var bmp = icon.ToBitmap();
            using var ms = new MemoryStream();
            
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            return new Bitmap(ms);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveIcon(int appId, Bitmap icon)
    {
        try
        {
            var path = Path.Combine(IconsDir, $"{appId}.png");
            
            // Конвертируем в PNG
            using var ms = new MemoryStream();
            icon.Save(ms);
            ms.Position = 0;
            
            using var fs = File.Create(path);
            ms.CopyTo(fs);
        }
        catch
        {
            // Игнорируем ошибки сохранения
        }
    }

    public static void DeleteIcon(int appId)
    {
        try
        {
            var path = Path.Combine(IconsDir, $"{appId}.png");
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Игнорируем ошибки
        }
    }

    public static bool HasCachedIcon(int appId)
    {
        var path = Path.Combine(IconsDir, $"{appId}.png");
        return File.Exists(path);
    }

    public static void ClearCache()
    {
        try
        {
            if (Directory.Exists(IconsDir))
            {
                foreach (var file in Directory.GetFiles(IconsDir, "*.png"))
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Игнорируем ошибки
        }
    }
}