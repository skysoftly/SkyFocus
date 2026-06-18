using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SkyFocus.Services;
using SkyFocus.ViewModels;
using SkyFocus.Views;

namespace SkyFocus;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<TrackingService>();
        services.AddSingleton<AppDbService>();

        
        services.AddSingleton<AppsListViewModel>();
        services.AddSingleton<AppInfoViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        var provider = services.BuildServiceProvider();
        
        var tracker = provider.GetRequiredService<TrackingService>();
        _ = tracker.StartAsync();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow();
            MainWindow.DataContext = provider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}