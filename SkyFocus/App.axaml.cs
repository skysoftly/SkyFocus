using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyFocus.Data;
using SkyFocus.Services;
using SkyFocus.ViewModels;
using SkyFocus.Views;

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
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadWindowSettings(SettingsService settings)
    {
        var state = settings.Get<string>("WindowState", "Normal");
        MainWindow?.WindowState = state == "Maximized" ? WindowState.Maximized : WindowState.Normal;

        var grid = MainWindow.FindControl<Grid>("TopGrid");
        if (grid != null && grid.ColumnDefinitions.Count >= 3)
        {
            // Загружаем как строки с *
            var col1 = settings.Get<string>("TopGrid_Col1", "*");
            var col2 = settings.Get<string>("TopGrid_Col2", "8");
            var col3 = settings.Get<string>("TopGrid_Col3", "2*");
        
            grid.ColumnDefinitions[0].Width = ParseGridLength(col1);
            grid.ColumnDefinitions[1].Width = ParseGridLength(col2);
            grid.ColumnDefinitions[2].Width = ParseGridLength(col3);
        }
    }

    private void SaveWindowSettings(SettingsService settings)
    {
        settings.Set("WindowState", MainWindow?.WindowState.ToString());


        var grid = MainWindow.FindControl<Grid>("TopGrid");
        if (grid != null && grid.ColumnDefinitions.Count >= 3)
        {
            settings.Set("TopGrid_Col1", GridLengthToString(grid.ColumnDefinitions[0].Width));
            settings.Set("TopGrid_Col2", GridLengthToString(grid.ColumnDefinitions[1].Width));
            settings.Set("TopGrid_Col3", GridLengthToString(grid.ColumnDefinitions[2].Width));
        }
    }


    private void ConfigureServices(ServiceCollection services)
    {
        // DbContext
        services.AddDbContext<AppDbContext>();

        // Сервисы
        services.AddSingleton<AppDbService>();
        services.AddSingleton<TrackingService>();
        services.AddSingleton<AppService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<IConfirmService, OverlayViewModel>();


        // ViewModels
        services.AddSingleton<WindowBarViewModel>();
        services.AddSingleton<OverlayViewModel>();
        services.AddSingleton<AppsListViewModel>();
        services.AddSingleton<AppInfoViewModel>();
        services.AddSingleton<ChartViewModel>();
        services.AddSingleton<MainWindowViewModel>();

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
}