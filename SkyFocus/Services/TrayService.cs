using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using SkyFocus.Views;

namespace SkyFocus.Services;

public class TrayService
{
    
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private TrayIcon? _tray;

    public TrayService(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
    }

    public void Init()
    {
        App.MainWindow?.Closing += (_, e) =>
        {
            e.Cancel = true;
            App.MainWindow.Hide();
        };

        _tray = new TrayIcon
        {
            Icon =  new WindowIcon(AssetLoader.Open(new Uri("avares://SkyFocus/Assets/icon.ico"))),
            ToolTipText = "SkyFocus",
            Menu = new NativeMenu()
        };

        var open = new NativeMenuItem("Открыть");
        var exit = new NativeMenuItem("Выход");

        open.Click += (_, _) => Open();
        exit.Click += (_, _) =>
        {
            App.MainWindow?.Hide();
            _tray.Dispose();
            _desktop.Shutdown();
        };

        _tray.Menu.Items.Add(open);
        _tray.Menu.Items.Add(exit);

        _tray.Clicked += (_, _) => Open();
    }

    private void Open()
    {
        App.MainWindow?.Show();
        App.MainWindow?.Activate();
    }
}