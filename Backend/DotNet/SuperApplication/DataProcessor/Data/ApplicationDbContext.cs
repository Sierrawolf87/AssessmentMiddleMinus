using DataProcessor.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Data;

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
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });
    }
}
