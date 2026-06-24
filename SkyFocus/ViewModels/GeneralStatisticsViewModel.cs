using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class GeneralStatisticsViewModel : ViewModelBase
{
    private readonly AppDbService _appDbService;
    private DateTime _lastUpdate = DateTime.MinValue;
    
    [ObservableProperty] private int _totalSecondsToday;
    [ObservableProperty] private int _totalSecondsWeek;
    [ObservableProperty] private int _totalSecondsMonth;
    
    [ObservableProperty] private int _percentChangeToday;
    [ObservableProperty] private int _percentChangeWeek;
    [ObservableProperty] private int _percentChangeMonth;
    
    [ObservableProperty] private bool _changeTodayUp;
    [ObservableProperty] private bool _changeWeekUp;
    [ObservableProperty] private bool _changeMonthUp;
    
    [ObservableProperty] private int _totalSecondsAll;

    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    
    public string ChangeTodayDisplay => $"{GetArrow(ChangeTodayUp)} {PercentChangeToday}%";
    public string ChangeWeekDisplay => $"{GetArrow(ChangeWeekUp)} {PercentChangeWeek}%";
    public string ChangeMonthDisplay => $"{GetArrow(ChangeMonthUp)} {PercentChangeMonth}%";
  

    private string GetArrow(bool isUp) => isUp ? "▲" : "▼";

    

    public GeneralStatisticsViewModel(AppDbService appDbService)
    {
        _appDbService = appDbService;

        _appDbService.DataChanged += OnDataChanged;
        
        
        _ = LoadStatistics();
    }

    
    private async void OnDataChanged(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if ((now - _lastUpdate).TotalMinutes < 1)
            return;
        
        _lastUpdate = now;
        await UpdateStats();
    }


    private async Task UpdateStats()
    {
        try
        {
            var today = DateTime.Today;
            
            // Получаем всю статистику
            var allStats = await _appDbService.GetAllStatsAsync();
            
            // Сегодня
            var todayStats = allStats.Where(s => s.Date.Date == today).ToList();
            TotalSecondsToday = todayStats.Sum(s => s.UsageTimeSeconds);
            
            
            var yesterday = today.AddDays(-1);
            var yesterdayStats = allStats.Where(s => s.Date.Date == yesterday).ToList();
            var yesterdaySeconds = yesterdayStats.Sum(s => s.UsageTimeSeconds);
            
            if (yesterdaySeconds > 0)
            {
                var change = (int)Math.Round(((double)(TotalSecondsToday - yesterdaySeconds) / yesterdaySeconds) * 100);
                PercentChangeToday = Math.Abs(change);
                ChangeTodayUp = change >= 0;
            }
            else
            {
                PercentChangeToday = 0;
                ChangeTodayUp = true;
            };
            
            
            // Эта неделя
            var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(6);
            
            var weekStats = allStats
                .Where(s => s.Date.Date >= weekStart && s.Date.Date <= weekEnd)
                .ToList();
            TotalSecondsWeek = weekStats.Sum(s => s.UsageTimeSeconds);
            
            
            var lastWeekStart = weekStart.AddDays(-7);
            var lastWeekEnd = weekStart.AddDays(-1);
            
            var lastWeekStats = allStats
                .Where(s => s.Date.Date >= lastWeekStart && s.Date.Date <= lastWeekEnd)
                .ToList();
            
            var lastWeekSeconds = lastWeekStats.Sum(s => s.UsageTimeSeconds);
            
            if (lastWeekSeconds > 0)
            {
                var change = (int)Math.Round(((double)(TotalSecondsWeek - lastWeekSeconds) / lastWeekSeconds) * 100);
                PercentChangeWeek = Math.Abs(change);
                ChangeWeekUp = change >= 0;
            }
            else
            {
                PercentChangeWeek = 0;
                ChangeWeekUp = true;
            }
            
            // этот месяц
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            var monthStats = allStats
                .Where(s => s.Date.Date >= monthStart && s.Date.Date <= monthEnd)
                .ToList();
            TotalSecondsMonth = monthStats.Sum(s => s.UsageTimeSeconds);
            
            var lastMonthStart = monthStart.AddMonths(-1);
            var lastMonthEnd = monthStart.AddDays(-1);
            
            var lastMonthStats = allStats
                .Where(s => s.Date.Date >= lastMonthStart && s.Date.Date <= lastMonthEnd)
                .ToList();
            
            var lastMonthSeconds = lastMonthStats.Sum(s => s.UsageTimeSeconds);
            
            if (lastMonthSeconds > 0)
            {
                var change = (int)Math.Round(((double)(TotalSecondsMonth - lastMonthSeconds) / lastMonthSeconds) * 100);
                PercentChangeMonth = Math.Abs(change);
                ChangeMonthUp = change >= 0;
            }
            else
            {
                PercentChangeMonth = 0;
                ChangeMonthUp = true;
            }
            
            
            // Все время
            TotalSecondsAll = allStats.Sum(s => s.UsageTimeSeconds);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обновления статистики: {ex.Message}");
        }
        
    }
    
    private async Task LoadStatistics()
    {
        await UpdateStats();
    }

}