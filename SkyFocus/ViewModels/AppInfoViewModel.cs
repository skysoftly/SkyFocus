using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;

namespace SkyFocus.ViewModels;

public partial class AppInfoViewModel : ViewModelBase
{
    [ObservableProperty] private AppRowDto? _selectedApp;
    
    [ObservableProperty] private bool _isBusy;
    
    
    public AppInfoViewModel(AppsListViewModel appsList)
    {
        appsList.SelectedAppChanged += selectedApp =>
        {
            SelectedApp = selectedApp;
        };
    }

    [RelayCommand]
    private async Task ToggleApp()
    {
        if (SelectedApp == null || IsBusy)
            return;

        IsBusy = true;

        try
        {
            if (SelectedApp.IsRunning)
            {
                var processes = Process.GetProcessesByName(SelectedApp.ProcessName);

                foreach (var p in processes)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch
                    {
                        // нет прав или процесс уже закрылся
                    }
                }
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedApp.Path,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch
                {
                    // не получилось открыть
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}