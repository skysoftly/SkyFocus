using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkyFocus.Data.Entities;

namespace SkyFocus.Data;

public class AppDbContext : DbContext
{
    public DbSet<AppEntity> Apps => Set<AppEntity>();
    public DbSet<DailyAppStatEntity> DailyStats => Set<DailyAppStatEntity>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite( $"Data Source={DbPath.GetPath()}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppEntity>(entity =>
        {
            entity.ToTable("App");
            entity.HasKey(a => a.Id);
            
            entity.Property(a => a.Name).IsRequired();
            entity.Property(a => a.Path).IsRequired();
            entity.HasIndex(a => a.Path).IsUnique();
            
            entity.Property(a => a.ProcessName).IsRequired();
            entity.Property(a => a.NoteText).HasMaxLength(250);
            
            entity.HasIndex(a => a.IsFavorite);

            
            entity.HasMany(a => a.DailyStats)
                .WithOne(d => d.App)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        
        modelBuilder.Entity<DailyAppStatEntity>(entity =>
        {
            entity.ToTable("DailyAppStat");
            entity.HasKey(d => d.Id);
            
            entity.Property(d => d.AppId)
                .IsRequired();
                
            entity.Property(d => d.Date)
                .IsRequired();

            entity.Property(d => d.UsageTimeSeconds)
                .IsRequired();

            entity.HasIndex(d => new { d.AppId, d.Date })
                .IsUnique();
            
            entity.HasOne(d => d.App)
                .WithMany(a => a.DailyStats)
                .HasForeignKey(d => d.AppId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}