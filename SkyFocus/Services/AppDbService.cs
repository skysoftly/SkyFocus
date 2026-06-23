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
    public async Task AddAsync(AppRowDto dto)
    {
        using var db = new AppDbContext();

        var entity = new AppEntity
        {
            Name = dto.Name,
            Path = dto.Path,
            ProcessName = dto.ProcessName
        };
        
        db.Apps.Add(entity);
        await db.SaveChangesAsync();
        
        dto.Id = entity.Id;
    }
    
    public async Task UpdateAppAsync(AppRowDto dto)
    {
        using var db = new AppDbContext();

        var entity = await db.Apps.FindAsync(dto.Id);
        
        if (entity == null) return;
        
        entity.Name = dto.Name;
        entity.Path = dto.Path;
        entity.NoteText = dto.NoteText;
        entity.LaunchCount = dto.LaunchCount;
        entity.ProcessName = dto.ProcessName;
        entity.IsFavorite = dto.IsFavorite;
        
        await db.SaveChangesAsync();
    }
    
    
    public async Task RemoveAppAsync(int id)
    {
        using var db = new AppDbContext();

        await db.Apps.Where(x => x.Id == id).ExecuteDeleteAsync();

        await db.SaveChangesAsync();
    }
    
    
    public async Task AddUsageTimeAsync(int appId, int secondsToAdd)
    {
        using var db = new AppDbContext();
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
    }
    
    
    public async Task<List<AppRowDto>> LoadAppsWithTodayStatsAsync()
    {
        using var db = new AppDbContext();
        var today = DateTime.Today;
        
        var apps = await db.Apps
            .Select(a => new AppRowDto
            {
                Id = a.Id,
                Name = a.Name,
                Path = a.Path,
                ProcessName = a.ProcessName,
                LaunchCount = a.LaunchCount,
                IsFavorite = a.IsFavorite,
                NoteText = a.NoteText,
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
        return await new AppDbContext().DailyStats
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
}