using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.DTOs;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class AppsListViewModel(AppService appService) : ViewModelBase
{
    public AppService AppService { get; } = appService;
    
    [RelayCommand]
    private async Task ToggleFavorite(AppRowDto app)
    {
        await AppService.ToggleFavorite(app);
    }
    
    [RelayCommand]
    private async Task AddApp()
    {
        await AppService.AddApp();
    }
}