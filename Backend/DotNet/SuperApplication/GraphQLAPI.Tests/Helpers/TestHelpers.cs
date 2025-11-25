using Microsoft.EntityFrameworkCore;
using SuperApplication.Shared.Data;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;

namespace GraphQLAPI.Tests.Helpers;

public static class TestHelpers
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static ApplicationDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Seeds the database with sample sensor reading data
    /// </summary>
    public static async Task SeedDatabase(ApplicationDbContext context)
    {
        var readings = new List<SensorReading>
        {
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.AirQuality,
                Name = SensorLocation.LivingRoom,
                Timestamp = DateTime.UtcNow.AddHours(-5),
                Co2 = 400,
                Pm25 = 10,
                Humidity = 50
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.AirQuality,
                Name = SensorLocation.Kitchen,
                Timestamp = DateTime.UtcNow.AddHours(-4),
                Co2 = 500,
                Pm25 = 15,
                Humidity = 60
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.Motion,
                Name = SensorLocation.Bedroom,
                Timestamp = DateTime.UtcNow.AddHours(-3),
                MotionDetected = true
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.Energy,
                Name = SensorLocation.Garage,
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Energy = 100.5
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.AirQuality,
                Name = SensorLocation.Office,
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Co2 = 450,
                Pm25 = 12,
                Humidity = 55
            }
        };

        context.SensorReadings.AddRange(readings);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a sample sensor reading for testing
    /// </summary>
    public static SensorReading CreateSampleReading()
    {
        return new SensorReading
        {
            Id = Guid.NewGuid(),
            Type = SensorType.AirQuality,
            Name = SensorLocation.LivingRoom,
            Timestamp = DateTime.UtcNow,
            Co2 = 420,
            Pm25 = 11,
            Humidity = 52
        };
    }
}
