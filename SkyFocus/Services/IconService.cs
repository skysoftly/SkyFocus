using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace SkyFocus.Services;

public static class IconService
{
    public static async Task<Bitmap?> GetIconAsync(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var icon = Icon.ExtractAssociatedIcon(path);

                if (icon == null)
                    return null;

                using var stream = new MemoryStream();
                icon.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        });
    }
}