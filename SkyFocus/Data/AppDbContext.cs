using Microsoft.EntityFrameworkCore;
using SkyFocus.Data.Entities;

namespace SkyFocus.Data;

public class AppDbContext : DbContext
{
    public DbSet<AppEntity> Apps => Set<AppEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath.GetPath()}"); 
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
            
        });
    }
}