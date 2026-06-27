using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    
    [ObservableProperty] private bool _isAutoStart;
    [ObservableProperty] private bool _isTrackingNotify;

    partial void OnIsAutoStartChanged(bool value)
    {
        if (value)
            AutoStartService.Enable();
        else
        {
            AutoStartService.Disable();
        }
    }

    partial void OnIsTrackingNotifyChanged(bool value)
    {
        _settingsService.Set("IsTrackingNotify", value);
    }

    public SettingsPageViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        IsTrackingNotify = _settingsService.Get("IsTrackingNotify", false);
    }
    
    public async Task Update()
    {
        IsAutoStart = AutoStartService.IsEnabled();
    }
}