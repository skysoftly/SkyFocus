using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
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
        
        
        CheckAutoStart();

        base.OnFrameworkInitializationCompleted();
    }

    
    private async void CheckAutoStart()
    {
        // Проверяем, был ли уже задан вопрос
        var settings = _serviceProvider?.GetService<SettingsService>();
        if (settings == null) return;

        var asked = settings.Get<bool?>("AutoStartAsked");
        if (asked != true)
        {
            // Первый запуск — показываем вопрос
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new ConfirmDialog("Добавить SkyFocus в автозагрузку?");
                var result = await dialog.ShowDialog<bool?>(App.MainWindow!);
            
                if (result == true)
                {
                    AutoStartService.Enable();
                }
            
                settings.Set("AutoStartAsked", true);
            });
        }
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
        services.AddScoped<TopAppsViewModel>();
        services.AddScoped<SettingsPageViewModel>();

        // Окна
        services.AddSingleton<MainWindow>(sp =>
            new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            });

        // services.AddSingleton<ConfirmDialog>();
        // services.AddSingleton<InfoDialog>();
        // services.AddSingleton<TextDialog>();
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
}