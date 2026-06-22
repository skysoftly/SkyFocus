using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Bitmap = Avalonia.Media.Imaging.Bitmap;


namespace SkyFocus.Services;

public static class IconService
{
    
    public static Bitmap? GetIcon(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(path);

            if (icon == null)
                return null;

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
}
