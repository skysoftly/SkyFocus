using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

public partial class ChartPageViewModel : ViewModelBase
{
    private readonly AppDbService _appDbService;
    private readonly SettingsService _settingsService;
    private readonly AppService _appService;
    
    [ObservableProperty] private ZoomAndPanMode _zoomMode = ZoomAndPanMode.None;
    [ObservableProperty] private Axis[] _xAxes = [];
    [ObservableProperty] private Axis[] _yAxes = [];
    [ObservableProperty] private ISeries[] _series = [];
    [ObservableProperty] private string[] _labels = [];
    [ObservableProperty] private bool _isLoading;

    // Удаляем массив _colors

    public List<PeriodOption> Periods { get; } =
    [
        new() { DisplayName = "7 дней", Type = PeriodType.Last7Days },
        new() { DisplayName = "30 дней", Type = PeriodType.Last30Days },
        new() { DisplayName = "За всё время", Type = PeriodType.AllTime }
    ];

    [ObservableProperty] private PeriodOption? _selectedPeriod;
    private readonly Dictionary<int, SKColor> _appColors = new();

    partial void OnSelectedPeriodChanged(PeriodOption? value)
    {
        if (value != null)
        {
            _settingsService.Set("SelectedPeriodType2", value.Type.ToString());
            _ = UpdateChart();
        }
    }

    public ChartPageViewModel(AppDbService appDbService, SettingsService settingsService, AppService appService)
    {
        _appDbService = appDbService;
        _settingsService = settingsService;
        _appService = appService;

        string savedPeriodType = _settingsService.Get("SelectedPeriodType2", PeriodType.Last7Days.ToString());
        SelectedPeriod = Periods.FirstOrDefault(p => p.Type.ToString() == savedPeriodType) ?? Periods[0];

        YAxes =
        [
            new Axis()
            {
                MinLimit = 0,
                Name = "Часы"
            }
        ];
    }

    // Генерация яркого цвета на основе ID
    private static SKColor GetRandomBrightColor(int seed)
    {
        var random = new Random(seed);
        
        double hue = random.NextDouble() * 360;
        double saturation = 0.7 + random.NextDouble() * 0.3; // 70-100%
        double lightness = 0.5 + random.NextDouble() * 0.3; // 50-80%
        
        return HslToRgb(hue, saturation, lightness);
    }

    private static SKColor HslToRgb(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;
        
        double r, g, b;
        
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }
        
        return new SKColor(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }

    public async Task UpdateChart()
    {
        if (SelectedPeriod == null || _appService.FilteredApps.Count == 0) return;

        IsLoading = true;

        try
        {
            var today = DateTime.Today;
            DateTime startDate, endDate;

            switch (SelectedPeriod.Type)
            {
                case PeriodType.Last7Days:
                    startDate = today.AddDays(-6);
                    endDate = today;
                    break;
                case PeriodType.Last30Days:
                    startDate = today.AddDays(-29);
                    endDate = today;
                    break;
                case PeriodType.AllTime:
                    startDate = await GetGlobalFirstDateAsync();
                    endDate = today;
                    break;
                default:
                    startDate = today.AddDays(-6);
                    endDate = today;
                    break;
            }

            var apps = _appService.FilteredApps.ToList();
            
            // Получаем статистику
            var allStats = new List<AppStatsDto>();
            foreach (var app in apps)
            {
                var stats = await _appDbService.GetStatsForAppByDatesAsync(app.Id, startDate, endDate);
                allStats.AddRange(stats);
            }

            // Группируем по месяцам для AllTime
            if (SelectedPeriod.Type == PeriodType.AllTime)
            {
                var monthlyStats = allStats
                    .GroupBy(s => new { s.AppId, Year = s.Date.Year, Month = s.Date.Month })
                    .Select(g => new AppStatsDto
                    {
                        AppId = g.Key.AppId,
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        UsageTimeSeconds = g.Sum(s => s.UsageTimeSeconds)
                    })
                    .ToList();

                var statsByApp = monthlyStats
                    .GroupBy(s => s.AppId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(s => s.Date, s => s.UsageTimeSeconds));

                var monthsCount = (int)((endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month) + 1;
                
                Labels = Enumerable.Range(0, monthsCount)
                    .Select(i => startDate.AddMonths(i).ToString("MMM yyyy"))
                    .ToArray();

                var seriesList = new List<ISeries>();

                foreach (var app in apps)
                {
                    // Генерируем цвет на основе ID приложения
                    if (!_appColors.TryGetValue(app.Id, out var color))
                    {
                        color = GetRandomBrightColor(app.Id + 12345);
                        _appColors[app.Id] = color;
                    }

                    var values = new double?[monthsCount];
                    bool hasFound = false;

                    if (statsByApp.TryGetValue(app.Id, out var appStats))
                    {
                        for (int i = 0; i < monthsCount; i++)
                        {
                            var date = startDate.AddMonths(i);
                            if (appStats.TryGetValue(date, out var seconds))
                            {
                                var hours = Math.Round(seconds / 3600.0, 1);
                                if (hours > 0)
                                {
                                    hasFound = true;
                                    values[i] = hours;
                                }
                                else if (hasFound)
                                {
                                    values[i] = 0;
                                }
                            }
                            else if (hasFound)
                            {
                                values[i] = 0;
                            }
                        }
                    }

                    if (values.Any(v => v.HasValue && v.Value > 0))
                    {
                        seriesList.Add(new LineSeries<double?>
                        {
                            Values = values,
                            Name = app.Name.Length > 15 ? app.Name.Substring(0, 15) + "..." : app.Name,
                            Fill = null,
                            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                            GeometrySize = 4,
                            LineSmoothness = 0.5,
                            GeometryFill = new SolidColorPaint(color),
                            GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 1 },
                        });
                    }
                }

                Series = seriesList.ToArray();
                XAxes =
                [
                    new Axis()
                    {
                        Labels = Labels,
                        LabelsRotation = 45,
                        MinLimit = -0.5,
                        MaxLimit = monthsCount - 0.5
                    }
                ];
            }
            else // Для 7 и 30 дней - по дням
            {
                var daysCount = (int)(endDate - startDate).TotalDays + 1;
                
                var statsByApp = allStats
                    .GroupBy(s => s.AppId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(s => s.Date.Date, s => s.UsageTimeSeconds));

                Labels = Enumerable.Range(0, daysCount)
                    .Select(i => GetDayLabel(startDate.AddDays(i)))
                    .ToArray();

                var seriesList = new List<ISeries>();

                foreach (var app in apps)
                {
                    // Генерируем цвет на основе ID приложения
                    if (!_appColors.TryGetValue(app.Id, out var color))
                    {
                        color = GetRandomBrightColor(app.Id + 12345);
                        _appColors[app.Id] = color;
                    }

                    var values = new double?[daysCount];
                    bool hasFound = false;

                    if (statsByApp.TryGetValue(app.Id, out var appStats))
                    {
                        for (int i = 0; i < daysCount; i++)
                        {
                            var date = startDate.AddDays(i);
                            if (appStats.TryGetValue(date.Date, out var seconds))
                            {
                                var hours = Math.Round(seconds / 3600.0, 1);
                                if (hours > 0)
                                {
                                    hasFound = true;
                                    values[i] = hours;
                                }
                                else if (hasFound)
                                {
                                    values[i] = 0;
                                }
                            }
                            else if (hasFound)
                            {
                                values[i] = 0;
                            }
                        }
                    }

                    if (values.Any(v => v.HasValue && v.Value > 0))
                    {
                        seriesList.Add(new LineSeries<double?>
                        {
                            Values = values,
                            Name = app.Name.Length > 15 ? app.Name.Substring(0, 15) + "..." : app.Name,
                            Fill = null,
                            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                            GeometrySize = 4,
                            LineSmoothness = 0.5,
                            GeometryFill = new SolidColorPaint(color),
                            GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 1 },
                        });
                    }
                }

                Series = seriesList.ToArray();
                XAxes =
                [
                    new Axis()
                    {
                        Labels = Labels,
                        LabelsRotation = daysCount > 15 ? 45 : 0,
                        MinLimit = -0.5,
                        MaxLimit = daysCount - 0.5
                    }
                ];
            }

            ZoomMode = SelectedPeriod?.Type == PeriodType.AllTime ? ZoomAndPanMode.X : ZoomAndPanMode.None;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chart error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<DateTime> GetGlobalFirstDateAsync()
    {
        try
        {
            var apps = _appService.FilteredApps.ToList();
            var dates = new List<DateTime>();
            
            foreach (var app in apps)
            {
                var date = await _appDbService.GetFirstDateForAppAsync(app.Id);
                if (date != DateTime.Today)
                    dates.Add(date);
            }
            
            return dates.Any() ? dates.Min() : DateTime.Today.AddDays(-30);
        }
        catch
        {
            return DateTime.Today.AddDays(-30);
        }
    }

    private string GetDayLabel(DateTime date)
    {
        return date.ToString("dd.MM");
    }
}