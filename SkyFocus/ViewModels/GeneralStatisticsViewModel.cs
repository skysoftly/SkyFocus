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
    
    [ObservableProperty] private bool _hasChangeToday;
    [ObservableProperty] private bool _hasChangeWeek;
    [ObservableProperty] private bool _hasChangeMonth;
    
    [ObservableProperty] private int _totalSecondsAll;

    public string ChangeTodayDisplay => HasChangeToday ? $"{GetArrow(ChangeTodayUp)} {PercentChangeToday}%" : "—";
    public string ChangeWeekDisplay => HasChangeWeek ? $"{GetArrow(ChangeWeekUp)} {PercentChangeWeek}%" : "—";
    public string ChangeMonthDisplay => HasChangeMonth ? $"{GetArrow(ChangeMonthUp)} {PercentChangeMonth}%" : "—";

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
        var allStats = await _appDbService.GetAllStatsAsync();
        
        // ==================== СЕГОДНЯ ====================
        var todayStats = allStats.Where(s => s.Date.Date == today).ToList();
        TotalSecondsToday = todayStats.Sum(s => s.UsageTimeSeconds);
        
        // ==================== НЕДЕЛЯ (ПН - ВС) ====================
        // Явно вычисляем понедельник
        var daysUntilMonday = ((int)today.DayOfWeek + 6) % 7; // Сколько дней до понедельника
        var weekStart = today.AddDays(-daysUntilMonday);
        var weekEnd = weekStart.AddDays(6); // Воскресенье
        
        var weekStats = allStats
            .Where(s => s.Date.Date >= weekStart && s.Date.Date <= weekEnd)
            .ToList();
        TotalSecondsWeek = weekStats.Sum(s => s.UsageTimeSeconds);
        
        // ==================== МЕСЯЦ ====================
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        
        var monthStats = allStats
            .Where(s => s.Date.Date >= monthStart && s.Date.Date <= monthEnd)
            .ToList();
        TotalSecondsMonth = monthStats.Sum(s => s.UsageTimeSeconds);
        
        // ==================== ВСЁ ВРЕМЯ ====================
        TotalSecondsAll = allStats.Sum(s => s.UsageTimeSeconds);
        
        // ==================== ПРОЦЕНТЫ ====================
        // Вчера
        var yesterday = today.AddDays(-1);
        var yesterdayStats = allStats.Where(s => s.Date.Date == yesterday).ToList();
        var yesterdaySeconds = yesterdayStats.Sum(s => s.UsageTimeSeconds);
        
        if (yesterdaySeconds == 0)
        {
            HasChangeToday = false;
            PercentChangeToday = 0;
            ChangeTodayUp = true;
        }
        else
        {
            var change = (int)Math.Round(((double)(TotalSecondsToday - yesterdaySeconds) / yesterdaySeconds) * 100);
            PercentChangeToday = Math.Abs(change);
            ChangeTodayUp = change >= 0;
            HasChangeToday = true;
        }
        
        // Прошлая неделя
        var lastWeekStart = weekStart.AddDays(-7);
        var lastWeekEnd = lastWeekStart.AddDays(6);
        var lastWeekStats = allStats
            .Where(s => s.Date.Date >= lastWeekStart && s.Date.Date <= lastWeekEnd)
            .ToList();
        var lastWeekSeconds = lastWeekStats.Sum(s => s.UsageTimeSeconds);
        
        if (lastWeekSeconds == 0)
        {
            HasChangeWeek = false;
            PercentChangeWeek = 0;
            ChangeWeekUp = true;
        }
        else
        {
            var change = (int)Math.Round(((double)(TotalSecondsWeek - lastWeekSeconds) / lastWeekSeconds) * 100);
            PercentChangeWeek = Math.Abs(change);
            ChangeWeekUp = change >= 0;
            HasChangeWeek = true;
        }
        
        // Прошлый месяц
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);
        var lastMonthStats = allStats
            .Where(s => s.Date.Date >= lastMonthStart && s.Date.Date <= lastMonthEnd)
            .ToList();
        var lastMonthSeconds = lastMonthStats.Sum(s => s.UsageTimeSeconds);
        
        if (lastMonthSeconds == 0)
        {
            HasChangeMonth = false;
            PercentChangeMonth = 0;
            ChangeMonthUp = true;
        }
        else
        {
            var change = (int)Math.Round(((double)(TotalSecondsMonth - lastMonthSeconds) / lastMonthSeconds) * 100);
            PercentChangeMonth = Math.Abs(change);
            ChangeMonthUp = change >= 0;
            HasChangeMonth = true;
        }
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