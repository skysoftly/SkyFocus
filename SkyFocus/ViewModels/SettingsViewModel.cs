using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkyFocus.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isVisible;
    
    [RelayCommand]
    private void Enable()
    {
        IsVisible = true;
    }
    
    [RelayCommand]
    private void Disable()
    {
        IsVisible = false;
    }
}