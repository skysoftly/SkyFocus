using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyFocus.Data;
using SkyFocus.Data.Entities;
using SkyFocus.DTOs;

namespace SkyFocus.Services;

public class AppDbService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    public event EventHandler? DataChanged;
    
    public AppDbService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
    
    public async Task AddAsync(AppRowDto dto)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = new AppEntity
        {
            Name = dto.Name,
            Path = dto.Path,
            ProcessName = dto.ProcessName,
            IconPath = dto.IconPath
        };
        
        db.Apps.Add(entity);
        await db.SaveChangesAsync();
        
        dto.Id = entity.Id;
    }
    
    public async Task UpdateAppAsync(AppRowDto dto)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = await db.Apps.FindAsync(dto.Id);
        
        if (entity == null) return;
        
        entity.Name = dto.Name;
        entity.Path = dto.Path;
        entity.NoteText = dto.NoteText;
        entity.ProcessName = dto.ProcessName;
        entity.IsFavorite = dto.IsFavorite;
        entity.IconPath = dto.IconPath;
        
        await db.SaveChangesAsync();
        
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
    
    
    public async Task RemoveAppAsync(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        await db.Apps.Where(x => x.Id == id).ExecuteDeleteAsync();

        await db.SaveChangesAsync();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
    
    
    public async Task AddUsageTimeAsync(int appId, int secondsToAdd)
    {
        using var db = _dbFactory.CreateDbContext();
        var today = DateTime.Today;
        
        var stat = await db.DailyStats.FirstOrDefaultAsync(s => s.AppId == appId && s.Date == today);

        if (stat == null)
        {
            stat = new DailyAppStatEntity
            {
                AppId = appId,
                Date = today,
                UsageTimeSeconds = secondsToAdd
            };
            db.DailyStats.Add(stat);
        }
        else
        {
            stat.UsageTimeSeconds += secondsToAdd;
        }
        
        await db.SaveChangesAsync();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
    
    
    public async Task<List<AppRowDto>> LoadAppsWithTodayStatsAsync()
    {
        using var db = _dbFactory.CreateDbContext();
        var today = DateTime.Today;
        
        var apps = await db.Apps
            .Select(a => new AppRowDto
            {
                Id = a.Id,
                Name = a.Name,
                Path = a.Path,
                ProcessName = a.ProcessName,
                IsFavorite = a.IsFavorite,
                NoteText = a.NoteText,
                IconPath = a.IconPath,
                UsageTimeSeconds = a.DailyStats
                    .Where(s => s.Date == today)
                    .Sum(s => s.UsageTimeSeconds)
            })
            .OrderByDescending(a => a.UsageTimeSeconds)
            .ToListAsync();
        
        return apps;
    }

    public async Task<List<AppStatsDto>> GetStatsForAppByDatesAsync(int appId, DateTime startDate, DateTime endDate)
    {
        using var db = _dbFactory.CreateDbContext();
        return await db.DailyStats
            .Where(s => s.AppId == appId && s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => s.Date)
            .Select(g => new AppStatsDto
            {
                AppId = appId,
                Date = g.Key,
                UsageTimeSeconds = g.Sum(s => s.UsageTimeSeconds)
            })
            .OrderBy(s => s.Date)
            .ToListAsync();
    }

    public async Task<DateTime> GetFirstDateForAppAsync(int appId)
    {
        using var db = _dbFactory.CreateDbContext();
        var stats = await db.DailyStats
            .Where(s => s.AppId == appId)
            .ToListAsync();
    
        if (stats.Any())
            return stats.Min(s => s.Date);
    
        return DateTime.Today;
    }
    
    public async Task<List<AppStatsDto>> GetAllStatsAsync()
    {
        using var db = _dbFactory.CreateDbContext();
        
        return await db.DailyStats
            .Select(s => new AppStatsDto
            {
                Id = s.Id,
                AppId = s.AppId,
                Date = s.Date,
                UsageTimeSeconds = s.UsageTimeSeconds
            })
            .ToListAsync();
    }
    
    public async Task<AppRowDto?> GetByPathAsync(string path)
    {
        using var db = _dbFactory.CreateDbContext();
    
        var entity = await db.Apps
            .FirstOrDefaultAsync(a => a.Path == path);
    
        if (entity == null)
            return null;
    
        return new AppRowDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Path = entity.Path,
            ProcessName = entity.ProcessName,
            IsFavorite = entity.IsFavorite,
            NoteText = entity.NoteText,
            IconPath = entity.IconPath
        };
    }
}