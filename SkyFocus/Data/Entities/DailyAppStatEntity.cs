using System;

namespace SkyFocus.Data.Entities;

public class DailyAppStatEntity
{
    public int Id { get; set; }
    
    public int AppId { get; set; }
    public AppEntity App { get; set; }
    
    public DateTime Date { get; set; }
    public int UsageTimeSeconds  { get; set; }
}