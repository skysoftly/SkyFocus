using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyFocus.Data;
using SkyFocus.Data.Entities;
using SkyFocus.Services;
using SkyFocus.ViewModels;
using SkyFocus.Views;
using SkyFocus.Views.MessageBox;

namespace SkyFocus;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var settings = _serviceProvider.GetRequiredService<SettingsService>();

            MainWindow = mainWindow;
            desktop.MainWindow = mainWindow;

            LoadWindowSettings(settings);
            mainWindow.Closing += (_, _) => SaveWindowSettings(settings);

            // Запускаем трекер
            var tracker = _serviceProvider.GetRequiredService<TrackingService>();
            _ = tracker.StartAsync();

            // Tray service
            var tray = new TrayService(desktop);
            tray.Init();

            // _ = GenerateTestDataWithTodayAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadWindowSettings(SettingsService settings)
    {
        var state = settings.Get<string>("WindowState", "Normal");
        MainWindow?.WindowState = state == "Maximized" ? WindowState.Maximized : WindowState.Normal;

        var grid = MainWindow?.FindControl<Grid>("TopGrid");
        if (grid != null && grid.ColumnDefinitions.Count >= 3)
        {
            var saved = settings.Get<string>("TopGridColumns", "* 8 2*");

            if (!string.IsNullOrEmpty(saved))
            {
                var parts = saved.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    grid.ColumnDefinitions[0].Width = ParseGridLength(parts[0]);
                    grid.ColumnDefinitions[1].Width = ParseGridLength(parts[1]);
                    grid.ColumnDefinitions[2].Width = ParseGridLength(parts[2]);
                }
            }
        }
    }

    private void SaveWindowSettings(SettingsService settings)
    {
        settings.Set("WindowState", MainWindow?.WindowState.ToString());


        var grid = MainWindow?.FindControl<Grid>("TopGrid");
        if (grid != null && grid.ColumnDefinitions.Count >= 3)
        {
            var col1 = GridLengthToString(grid.ColumnDefinitions[0].Width);
            var col2 = GridLengthToString(grid.ColumnDefinitions[1].Width);
            var col3 = GridLengthToString(grid.ColumnDefinitions[2].Width);

            // Сохраняем как одну строку
            settings.Set("TopGridColumns", $"{col1} {col2} {col3}");
        }
    }


    private void ConfigureServices(ServiceCollection services)
    {
        // DbContext
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={DbPath.GetPath()}"));


        // Сервисы
        services.AddScoped<AppDbService>();
        services.AddSingleton<TrackingService>();
        services.AddScoped<AppService>();
        services.AddSingleton<SettingsService>();


        // ViewModels
        services.AddScoped<WindowBarViewModel>();
        services.AddScoped<AppsListViewModel>();
        services.AddScoped<AppInfoViewModel>();
        services.AddScoped<ChartViewModel>();
        services.AddScoped<MainWindowViewModel>();
        services.AddScoped<GeneralStatisticsViewModel>();
        services.AddScoped<ChartPageViewModel>();
        services.AddScoped<MainPageViewModel>();

        // Окна
        services.AddSingleton<MainWindow>(sp =>
            new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            });
    }

    private string GridLengthToString(GridLength length)
    {
        if (length.IsStar)
        {
            if (length.Value == 1)
                return "*";
            return $"{length.Value}*";
        }

        if (length.IsAbsolute)
            return $"{length.Value}";
        if (length.IsAuto)
            return "Auto";
        return "*";
    }

    private GridLength ParseGridLength(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new GridLength(1, GridUnitType.Star);

        if (value.EndsWith("*"))
        {
            var num = value.Replace("*", "");
            if (string.IsNullOrEmpty(num))
                return new GridLength(1, GridUnitType.Star);
            if (double.TryParse(num, out double d))
                return new GridLength(d, GridUnitType.Star);
        }

        if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            return GridLength.Auto;

        if (double.TryParse(value, out double pixel))
            return new GridLength(pixel, GridUnitType.Pixel);

        return new GridLength(1, GridUnitType.Star);
    }

    public async Task GenerateTestDataWithTodayAsync()
    {
        var random = new Random();
        var appIds = new[] { 2, 3, 7 };
        var startDate = new DateTime(2025, 1, 1);
        var endDate = DateTime.Today; // 2026-06-24 - автоматически

        using var db = new AppDbContext();

        // Очищаем старые данные (опционально)
        db.DailyStats.RemoveRange(db.DailyStats);
        await db.SaveChangesAsync();

        var stats = new List<DailyAppStatEntity>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // Пропускаем случайные дни (30% дней пропускаем)
            if (random.NextDouble() < 0.3 && date != endDate)
                continue;

            foreach (var appId in appIds)
            {
                // Иногда приложение не использовалось в этот день (20% пропуск)
                if (random.NextDouble() < 0.2)
                    continue;

                // Для сегодняшнего дня - особые значения
                int maxSeconds;
                if (date == DateTime.Today) // Сегодня 2026-06-24
                {
                    // Для сегодня делаем реалистичные значения
                    maxSeconds = random.Next(3600, 21600);
                    // if (appId == 2)
                    //     maxSeconds = random.Next(3600, 21600); // 1-6 часов (рабочее приложение)
                    // else if (appId == 3)
                    //     maxSeconds = random.Next(600, 7200); // 10 мин - 2 часа
                    // else
                    //     maxSeconds = random.Next(300, 1800); // 5-30 минут
                }
                else
                {
                    // Обычная генерация для прошлых дней
                    double chance = random.NextDouble();

                    if (chance < 0.1)
                        maxSeconds = random.Next(43200, 86400);
                    else if (chance < 0.3)
                        maxSeconds = random.Next(14400, 43200);
                    else if (chance < 0.6)
                        maxSeconds = random.Next(3600, 14400);
                    else if (chance < 0.85)
                        maxSeconds = random.Next(600, 3600);
                    else
                        maxSeconds = random.Next(0, 600);
                }

                var usageTime = random.Next(0, maxSeconds);

                // Иногда большие значения для реалистичности (только не для сегодня)
                if (date != DateTime.Today && random.NextDouble() < 0.05)
                    usageTime = random.Next(72000, 86400);

                stats.Add(new DailyAppStatEntity
                {
                    AppId = appId,
                    Date = date,
                    UsageTimeSeconds = usageTime
                });
            }
        }

        // Добавляем пакетами по 1000 записей
        int batchSize = 1000;
        for (int i = 0; i < stats.Count; i += batchSize)
        {
            var batch = stats.Skip(i).Take(batchSize);
            db.DailyStats.AddRange(batch);
            await db.SaveChangesAsync();
            Console.WriteLine($"Добавлено записей: {i + batch.Count()}/{stats.Count}");
        }

        Console.WriteLine($"Всего добавлено: {stats.Count} записей");
        Console.WriteLine($"Данные за сегодня ({DateTime.Today:yyyy-MM-dd}) включены!");
    }
}