using System;

namespace SkyFocus.DTOs;

public class AppStatsDto
{
    public int LaunchCount { get; set; }
    public int Id { get; set; }
    public int AppId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsFavorite { get; set; }
    
    public int UsageTimeSeconds  { get; set; }
}