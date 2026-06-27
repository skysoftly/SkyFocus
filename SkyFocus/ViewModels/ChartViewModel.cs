using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkyFocus.DTOs;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class ChartViewModel : ViewModelBase
{
    private readonly AppDbService _appDbService;
    private readonly SettingsService _settingsService;
    private ObservableCollection<DayUsage> _days = [];

    [ObservableProperty] private ZoomAndPanMode _zoomMode = ZoomAndPanMode.None;
    [ObservableProperty] private Axis[] _xAxes = [];
    [ObservableProperty] private Axis[] _yAxes = [];
    [ObservableProperty] ISeries[] _series = [];
    [ObservableProperty] string[] _labels = [];

    private int _appId;

    public List<PeriodOption> Periods { get; } =
    [
        new() { DisplayName = "Эта неделя (ПН-ВС)", Type = PeriodType.CalendarWeek },
        new() { DisplayName = "Последние 7 дней", Type = PeriodType.Last7Days },
        new() { DisplayName = "Этот месяц", Type = PeriodType.CalendarMonth },
        new() { DisplayName = "Последние 30 дней", Type = PeriodType.Last30Days },
        new() { DisplayName = "За всё время", Type = PeriodType.AllTime }
    ];


    [ObservableProperty] private PeriodOption? _selectedPeriod;


    partial void OnSelectedPeriodChanged(PeriodOption? value)
    {
        if (value != null)
        {
            _settingsService.Set("SelectedPeriodType", value.Type.ToString());
            
            _ = UpdateChart(_appId);
        }
    }

    public ChartViewModel(AppDbService appDbService, SettingsService settingsService)
    {
        _appDbService = appDbService;
        _settingsService = settingsService;
        
        string savedPeriodType = _settingsService.Get("SelectedPeriodType", PeriodType.CalendarWeek.ToString());
        SelectedPeriod = Periods.FirstOrDefault(p => p.Type.ToString() == savedPeriodType) ?? Periods[0];
        
        _appDbService.DataChanged += AppDbServiceOnDataChanged;
        
        
        YAxes =
        [
            new Axis()
            {
                MinLimit = 0
            }
        ];
    }

    private void AppDbServiceOnDataChanged(object? sender, EventArgs e)
    {
        _ = UpdateChart(_appId);
    }


    public async Task UpdateChart(int appId)
    {
        _appId = appId;

        if (SelectedPeriod == null) return;

        var today = DateTime.Today;
        DateTime startDate;
        DateTime endDate;
        int daysCount;
        

        switch (SelectedPeriod.Type)
        {
            case PeriodType.CalendarWeek:
                var daysUntilMonday = ((int)today.DayOfWeek + 6) % 7; // Сколько дней до понедельника
                var weekStart = today.AddDays(-daysUntilMonday);
                var weekEnd = weekStart.AddDays(6);
                startDate = weekStart;
                endDate = weekEnd;
                break;

            case PeriodType.Last7Days:
                startDate = today.AddDays(-6);
                endDate = today;
                break;

            case PeriodType.CalendarMonth:
                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                break;

            case PeriodType.Last30Days:
                startDate = today.AddDays(-29);
                endDate = today;
                break;

            case PeriodType.AllTime:
                var d = await _appDbService.GetFirstDateForAppAsync(appId);
                startDate = d;
                endDate = today;
                break;
            
            default:
                startDate = today.AddDays(-6);
                endDate = today;
                break;
            
            
        }
        daysCount = (int)(endDate - startDate).TotalDays + 1;
        var stats = await _appDbService.GetStatsForAppByDatesAsync(appId, startDate, endDate);
        
        _days.Clear();
        for (int i = 0; i < daysCount; i++)
        {
            var date = startDate.AddDays(i);
            var stat = stats.FirstOrDefault(s => s.Date.Date == date.Date);
        
            double hours = stat?.UsageTimeSeconds / 3600.0 ?? 0;
            hours = Math.Round(hours, 1);
        
            _days.Add(new DayUsage
            {
                Day = GetDayLabel(date, i, daysCount),
                Hours = hours
            });
        }
        
        Labels = _days.Select(d => d.Day).ToArray();
        
        Series =
        [
            new ColumnSeries<double>
            {
                Values = _days.Select(d => d.Hours).ToArray(),
                Name = "Часы использования",
                Fill = new LinearGradientPaint(
                    [
                        SKColor.Parse("#7858a7"),
                        SKColor.Parse("#413159")
                    ],
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)
                ),
                MaxBarWidth = 50,
                Rx = 4,
                Ry = 4
            }
        ];
        
        XAxes =
        [
            new Axis()
            {
                Labels = Labels,
                LabelsRotation = 0,
                MinLimit = -1,
                MaxLimit = _days.Count   
            }
        ];
        
        ZoomMode = SelectedPeriod?.Type == PeriodType.AllTime ? ZoomAndPanMode.X : ZoomAndPanMode.None;
    }

    private string GetDayLabel(DateTime date, int index, int totalDays)
    {
        if (SelectedPeriod == null) return date.ToString("dd.MM");

        if (totalDays <= 7)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Пн",
                DayOfWeek.Tuesday => "Вт",
                DayOfWeek.Wednesday => "Ср",
                DayOfWeek.Thursday => "Чт",
                DayOfWeek.Friday => "Пт",
                DayOfWeek.Saturday => "Сб",
                DayOfWeek.Sunday => "Вс",
                _ => date.ToString("ddd")
            };
        }
        if (totalDays <= 31)
        {
            return date.ToString("dd.MM");
        }

        return date.ToString("MM.dd.yyyy");
    }
}

public class PeriodOption
{
    public string DisplayName { get; set; } = string.Empty;
    public PeriodType Type { get; set; }
}

public enum PeriodType
{
    Today,
    CalendarWeek,
    Last7Days,
    CalendarMonth,
    Last30Days,
    AllTime
}