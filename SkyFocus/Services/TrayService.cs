using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace SkyFocus.Services;

public class TrayService
{
    
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly Window _window;
    private TrayIcon? _tray;

    public TrayService(IClassicDesktopStyleApplicationLifetime desktop, Window window)
    {
        _desktop = desktop;
        _window = window;
    }

    public void Init()
    {
        _window.Closing += (_, e) =>
        {
            e.Cancel = true;
            _window.Hide();
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
        exit.Click += (_, _) => _desktop.Shutdown();

        _tray.Menu.Items.Add(open);
        _tray.Menu.Items.Add(exit);

        _tray.Clicked += (_, _) => Open();
    }

    private void Open()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }
}