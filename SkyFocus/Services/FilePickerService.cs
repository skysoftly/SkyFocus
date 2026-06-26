using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace SkyFocus.Services;

public static class FilePickerService
{
    public static async Task<string?> PickExeFileAsync()
    {
        try
        {
            // 👇 КЛЮЧЕВОЙ МОМЕНТ: запускаем на UI потоке принудительно
            return await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // Небольшая задержка, чтобы UI успел "продышаться"
                await Task.Delay(50);
                
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
                        });

                return files.Count > 0 ? files[0].Path.LocalPath : null;
            }).ConfigureAwait(false);
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
            return await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(50);
                
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
                        });

                return files.Count > 0 ? files[0].Path.LocalPath : null;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }
    }

    public static async Task<List<string>?> PickExeFilesAsync()
    {
        try
        {
            return await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(50);
                
                var files = await App.MainWindow!.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title = "Выберите приложения",
                            AllowMultiple = true,
                            FileTypeFilter = new[]
                            {
                                new FilePickerFileType("Исполняемые файлы")
                                {
                                    Patterns = new[] { "*.exe", "*.url" }
                                }
                            }
                        });

                if (files.Count == 0) return null;
                
                var filePaths = new List<string>();
                foreach (var file in files)
                {
                    filePaths.Add(file.Path.LocalPath);
                }
                return filePaths;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return null;
        }
    }
}