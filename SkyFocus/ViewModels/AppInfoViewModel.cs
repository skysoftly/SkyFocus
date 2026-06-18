using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;

namespace SkyFocus.ViewModels;

public partial class AppInfoViewModel : ViewModelBase
{
    [ObservableProperty] private AppRowDto? _selectedApp;

    public AppInfoViewModel(AppsListViewModel appsList)
    {
        appsList.SelectedAppChanged += selectedApp =>
        {
            SelectedApp = selectedApp;
        };
    }

    [RelayCommand]
    private void ToggleApp()
    {
        
    }
}