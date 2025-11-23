using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace SuperApplication.Shared.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<SensorReading> SensorReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                .IsRequired();
            entity.Property(e => e.Name)
                .IsRequired();
        });
    }
}
