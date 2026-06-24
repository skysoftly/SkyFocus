using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace SkyFocus.Services;

public static class FilePickerService
{
    public static async Task<string?> PickExeFileAsync()
    {
        try
        {
            await Task.Delay(100);
            
            var files = await App.MainWindow!.StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Выберите приложение",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("Исполняемые файлы")
                            {
                                Patterns = new[] { "*.exe", "*.url" }
                            }
                        }
                    })
                .ConfigureAwait(false);

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }
    }
    
    public static async Task<string?> PickImageFileAsync()
    {
        try
        {
            await Task.Delay(100);
            
            var files = await App.MainWindow!.StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Выберите изображение",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("Изображения")
                            {
                                Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                            }
                        }
                    })
                .ConfigureAwait(false);

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }
    }
}