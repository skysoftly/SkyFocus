using System.Collections.Generic;

namespace SkyFocus.Data.Entities;

public class AppEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int LaunchCount { get; set; }
    public bool IsFavorite { get; set; }

    public ICollection<DailyAppStatEntity> DailyStats { get; set; } = new List<DailyAppStatEntity>();
}