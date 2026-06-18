using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;

namespace SkyFocus.ViewModels;

public partial class AppsListViewModel : ViewModelBase
{
    public ObservableCollection<AppRowDto> Apps { get; set; }

    [ObservableProperty] private AppRowDto _selectedApp;

    public AppsListViewModel()
    {
        Apps = new ObservableCollection<AppRowDto>()
        {
            new()
            {
                Name = "App1",
                Path = "C:\\Users\\skyso\\App1",
                IsFavorite = true,
            },
            new()
            {
                Name = "App2",
                Path = "C:\\Users\\skyso\\App2",
                IsRunning= true,
            },
            new()
            {
                Name = "App3",
                Path = "C:\\Users\\skyso\\App3"
            }
        };
    }

    [RelayCommand]
    private async Task ToggleFavorite(AppRowDto app)
    {
        
    }

}