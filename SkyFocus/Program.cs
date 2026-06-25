using Avalonia;
using System;
using System.Threading;

namespace SkyFocus;

sealed class Program
{
    private static Mutex? _mutex;
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        const string appId = "SkyFocus_App_Mutex";

        _mutex = new Mutex(true, appId, out bool isNewInstance);

        if (!isNewInstance)
        {
            // уже запущено → просто выходим
            return;
        }
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}