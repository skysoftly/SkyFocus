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
    public async Task<List<AppRowDto>> LoadAppsAsync()
    {
        using var db = new AppDbContext();

        return await db.Apps.Select(x => new AppRowDto
        {
            Id = x.Id,
            Name = x.Name,
            Path = x.Path,
            ProcessName = x.ProcessName,
            IsFavorite = x.IsFavorite
        }).ToListAsync();
    }
    
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
}