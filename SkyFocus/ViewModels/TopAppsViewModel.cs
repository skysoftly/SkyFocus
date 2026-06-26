using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.DTOs;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class TopAppsViewModel : ViewModelBase
{
    private readonly AppDbService _appDbService;
    private readonly SettingsService _settingsService;

    [ObservableProperty] private ObservableCollection<TopApp> _topApps = new();

    public List<PeriodOption> Periods { get; } =
    [
        new() { DisplayName = "За Сегодня", Type = PeriodType.Today },
        new() { DisplayName = "За неделю", Type = PeriodType.CalendarWeek },
        new() { DisplayName = "За месяц", Type = PeriodType.CalendarMonth },
        new() { DisplayName = "За всё время", Type = PeriodType.AllTime }
    ];


    partial void OnSelectedPeriodChanged(PeriodOption? value)
    {
        if (value != null)
        {
            _settingsService.Set("SelectedPeriodType3", value.Type.ToString());
            
            _ = LoadTopAppsAsync();
        }
    }
    
    
    [ObservableProperty] private PeriodOption? _selectedPeriod;


    public TopAppsViewModel(AppDbService appDbService, SettingsService settingsService)
    {
        _appDbService = appDbService;
        _settingsService = settingsService;
        
        string savedPeriodType = _settingsService.Get("SelectedPeriodType3", PeriodType.CalendarWeek.ToString());
        SelectedPeriod = Periods.FirstOrDefault(p => p.Type.ToString() == savedPeriodType) ?? Periods[0];


        
        appDbService.DataChanged += AppDbServiceOnDataChanged;
    }

    private void AppDbServiceOnDataChanged(object? sender, EventArgs e)
    {
        _ = LoadTopAppsAsync();
    }

    public async Task LoadTopAppsAsync()
    {
        
        var today = DateTime.Today;
        DateTime startDate;

        switch (SelectedPeriod?.Type)
        {
            case PeriodType.Today:
                startDate = today;
                break;
            case PeriodType.CalendarWeek:
                startDate = today.AddDays(-7);
                break;
            case PeriodType.CalendarMonth:
                startDate = today.AddDays(-30);
                break;
            case PeriodType.AllTime:
                startDate = DateTime.MinValue;
                break;
            default:
                startDate = today.AddDays(-30);
                break;
        }

        
        var apps = await _appDbService.GetTopAppsAsync(5, startDate, today);
        
        
        
        TopApps.Clear();
        int i = 0;
        foreach (var app in apps)
        {
            i += 1;
            
            var stats = await _appDbService.GetStatsForAppByDatesAsync(app.Id, startDate, today);
            var totalSeconds = stats.Sum(s => s.UsageTimeSeconds);
            
            var topApp = new TopApp()
            {
                Rank = i,
                App = app,
                TotalSeconds = totalSeconds
            };

            topApp.App.Icon = IconService.GetIconForApp(topApp.App.Id, topApp.App.Path);
            
            TopApps.Add(topApp);
        }
    }
}