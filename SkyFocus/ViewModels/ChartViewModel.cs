using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkyFocus.DTOs;
using SkyFocus.Models;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class ChartViewModel : ViewModelBase
{
    private AppDbService AppDbService { get; }
    private ObservableCollection<DayUsage> Days { get; set; } = [];


    public ISeries[] Series { get; private set; } = [];
    public string[] Labels { get; private set; } = [];

    private int _appId;
    
    public List<PeriodOption> Periods { get; } =
    [
        new() { DisplayName = "Эта неделя (ПН-ВС)", Type = PeriodType.CalendarWeek },
        new() { DisplayName = "Последние 7 дней", Type = PeriodType.Last7Days },
        new() { DisplayName = "Этот месяц", Type = PeriodType.CalendarMonth },
        new() { DisplayName = "Последние 30 дней", Type = PeriodType.Last30Days }
    ];
    
    
    [ObservableProperty]
    private PeriodOption? _selectedPeriod;

    partial void OnSelectedPeriodChanged(PeriodOption? value)
    {
        if (value != null)
            _ = UpdateChart(_appId);
    }

    public ChartViewModel(AppDbService appDbService)
    {
        AppDbService = appDbService;
        SelectedPeriod = Periods.FirstOrDefault();
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
                startDate = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                endDate = startDate.AddDays(6);
                daysCount = 7;
                break;
                
            case PeriodType.Last7Days:
                startDate = today.AddDays(-6);
                endDate = today;
                daysCount = 7;
                break;
                
            case PeriodType.CalendarMonth:
                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                daysCount = DateTime.DaysInMonth(today.Year, today.Month);
                break;
                
            case PeriodType.Last30Days:
                startDate = today.AddDays(-29);
                endDate = today;
                daysCount = 30;
                break;
                
            default:
                startDate = today.AddDays(-6);
                endDate = today;
                daysCount = 7;
                break;
        }
        
        var stats = await AppDbService.GetStatsForAppByDatesAsync(appId, startDate, endDate);

        var data = new List<DayUsage>();

        for (int i = 0; i < daysCount; i++)
        {
            var date = startDate.AddDays(i);
            var stat = stats.FirstOrDefault(s => s.Date.Date == date.Date);

            double hours = stat?.UsageTimeSeconds / 3600.0 ?? 0;
            hours = Math.Round(hours, 1);

            data.Add(new DayUsage
            {
                Day = GetDayLabel(date, i, daysCount),
                Hours = hours
            });
        }

        Days.Clear();
        foreach (var item in data)
            Days.Add(item);

        Labels = Days.Select(d => d.Day).ToArray();

        Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = Days.Select(d => d.Hours).ToArray(),
                Name = "Часы использования",
                Fill = new LinearGradientPaint(
                    new[]
                    {
                        SKColor.Parse("#7858a7"),
                        SKColor.Parse("#413159")
                    },
                    new SKPoint(0, 0),
                    new SKPoint(0, 1)
                ),
                MaxBarWidth = 50,
                Rx = 4,
                Ry = 4
            }
        };

        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(Labels));
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
        else
        {
            return date.ToString("dd.MM");
        }
    }
}


public class PeriodOption
{
    public string DisplayName { get; set; } = string.Empty;
    public PeriodType Type { get; set; }
}

public enum PeriodType
{
    CalendarWeek,
    Last7Days,
    CalendarMonth,
    Last30Days
}