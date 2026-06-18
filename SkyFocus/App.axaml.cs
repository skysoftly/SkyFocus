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
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<TrackingService>();
        
        services.AddSingleton<AppsListViewModel>();
        services.AddSingleton<AppInfoViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        var provider = services.BuildServiceProvider();
        
        var tracker = provider.GetRequiredService<TrackingService>();
        _ = tracker.StartAsync();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}