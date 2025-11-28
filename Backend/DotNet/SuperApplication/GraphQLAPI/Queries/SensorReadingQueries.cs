using SuperApplication.Shared.Data;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using GraphQLAPI.Types;

namespace GraphQLAPI.Queries;

/// <summary>
/// GraphQL queries for sensor reading data
/// </summary>
public class SensorReadingQueries
{
    /// <summary>
    /// Get paginated list of sensor readings with filtering and sorting support
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>Queryable collection of sensor readings</returns>
    [UsePaging(MaxPageSize = 200, DefaultPageSize = 20, IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SensorReading> GetSensorReadings(
        [Service] ApplicationDbContext context,
        [Service] ILogger<SensorReadingQueries> logger)
    {
        logger.LogDebug("Fetching sensor readings queryable");
        return context.SensorReadings.AsNoTracking();
    }
    
    /// <summary>
    /// Get a single sensor reading by ID
    /// </summary>
    /// <param name="id">The unique identifier of the sensor reading</param>
    /// <param name="context">Database context</param>
    /// <returns>Sensor reading if found, null otherwise</returns>
    [UseProjection]
    public async Task<SensorReading?> GetSensorReadingById(
        Guid id, 
        [Service] ApplicationDbContext context,
        [Service] ILogger<SensorReadingQueries> logger)
    {
        logger.LogDebug("Fetching sensor reading by ID: {Id}", id);
        return await context.SensorReadings
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }
    
    /// <summary>
    /// Get aggregated statistics for sensor readings with optional filtering
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="type">Optional filter by sensor type</param>
    /// <param name="name">Optional filter by sensor name</param>
    /// <param name="startDate">Optional filter for readings after this date</param>
    /// <param name="endDate">Optional filter for readings before this date</param>
    /// <returns>Aggregated statistics</returns>
    public async Task<SensorReadingStats> GetSensorReadingStats(
        [Service] ApplicationDbContext context,
        [Service] ILogger<SensorReadingQueries> logger,
        SensorType? type = null,
        SensorLocation? name = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        logger.LogInformation("Calculating sensor reading stats. Filters - Type: {Type}, Name: {Name}, Start: {Start}, End: {End}", 
            type, name, startDate, endDate);

        var query = context.SensorReadings.AsNoTracking().AsQueryable();
        
        // Apply filters
        if (type.HasValue)
        {
            query = query.Where(sr => sr.Type == type.Value);
        }
        
        if (name.HasValue)
        {
            query = query.Where(sr => sr.Name == name.Value);
        }
        
        if (startDate.HasValue)
        {
            query = query.Where(sr => sr.Timestamp >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(sr => sr.Timestamp <= endDate.Value);
        }
        
        // Calculate statistics
        var stats = new SensorReadingStats
        {
            TotalCount = await query.CountAsync(),
            AverageCo2 = await query.Where(sr => sr.Co2.HasValue).AverageAsync(sr => (double?)sr.Co2),
            AveragePm25 = await query.Where(sr => sr.Pm25.HasValue).AverageAsync(sr => (double?)sr.Pm25),
            AverageHumidity = await query.Where(sr => sr.Humidity.HasValue).AverageAsync(sr => (double?)sr.Humidity),
            AverageEnergy = await query.Where(sr => sr.Energy.HasValue).AverageAsync(sr => sr.Energy),
            MaxCo2 = await query.Where(sr => sr.Co2.HasValue).MaxAsync(sr => (int?)sr.Co2),
            MinCo2 = await query.Where(sr => sr.Co2.HasValue).MinAsync(sr => (int?)sr.Co2),
            MaxPm25 = await query.Where(sr => sr.Pm25.HasValue).MaxAsync(sr => (int?)sr.Pm25),
            MinPm25 = await query.Where(sr => sr.Pm25.HasValue).MinAsync(sr => (int?)sr.Pm25),
            MotionDetectedCount = await query.CountAsync(sr => sr.MotionDetected == true)
        };
        
        return stats;
    }
    
    /// <summary>
    /// Get sensor readings aggregated by configurable intervals for efficient chart rendering
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="type">Optional filter by sensor type</param>
    /// <param name="name">Optional filter by sensor name</param>
    /// <param name="startDate">Optional filter for readings after this date</param>
    /// <param name="endDate">Optional filter for readings before this date</param>
    /// <param name="intervalMinutes">Aggregation interval in minutes (default: 1). Supported: 1, 5, 60, 360, 1440</param>
    /// <returns>List of aggregated readings grouped by the specified interval</returns>
    public async Task<List<AggregatedSensorReading>> GetAggregatedSensorReadings(
        [Service] ApplicationDbContext context,
        [Service] ILogger<SensorReadingQueries> logger,
        SensorType? type = null,
        SensorLocation? name = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int intervalMinutes = 1)
    {
        logger.LogInformation("Fetching aggregated sensor readings. Interval: {Interval}min, Filters - Type: {Type}, Name: {Name}, Start: {Start}, End: {End}", 
            intervalMinutes, type, name, startDate, endDate);
        
        var query = context.SensorReadings.AsNoTracking().AsQueryable();
        
        // Apply filters
        if (type.HasValue)
        {
            query = query.Where(sr => sr.Type == type.Value);
        }
        
        if (name.HasValue)
        {
            query = query.Where(sr => sr.Name == name.Value);
        }
        
        if (startDate.HasValue)
        {
            query = query.Where(sr => sr.Timestamp >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(sr => sr.Timestamp <= endDate.Value);
        }
        
        // Group by interval and aggregate
        // For intervals >= 1 day (1440 minutes), group by date only
        // For intervals >= 1 hour but < 1 day, group by hour intervals
        // For intervals < 1 hour, group by minute intervals
        
        IQueryable<AggregatedSensorReading> aggregated;
        
        if (intervalMinutes >= 1440) // 1 day or more
        {
            var dayInterval = intervalMinutes / 1440;
            aggregated = query
                .GroupBy(sr => new
                {
                    Year = sr.Timestamp.Year,
                    Month = sr.Timestamp.Month,
                    Day = sr.Timestamp.Day / dayInterval * dayInterval
                })
                .Select(g => new AggregatedSensorReading
                {
                    Timestamp = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, 0, 0, 0),
                    AverageCo2 = g.Where(sr => sr.Co2.HasValue).Average(sr => (double?)sr.Co2),
                    AveragePm25 = g.Where(sr => sr.Pm25.HasValue).Average(sr => (double?)sr.Pm25),
                    AverageHumidity = g.Where(sr => sr.Humidity.HasValue).Average(sr => (double?)sr.Humidity),
                    AverageEnergy = g.Where(sr => sr.Energy.HasValue).Average(sr => sr.Energy),
                    Count = g.Count()
                })
                .OrderBy(r => r.Timestamp);
        }
        else if (intervalMinutes >= 60) // Hour intervals
        {
            var hourInterval = intervalMinutes / 60;
            aggregated = query
                .GroupBy(sr => new
                {
                    Year = sr.Timestamp.Year,
                    Month = sr.Timestamp.Month,
                    Day = sr.Timestamp.Day,
                    Hour = sr.Timestamp.Hour / hourInterval * hourInterval
                })
                .Select(g => new AggregatedSensorReading
                {
                    Timestamp = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    AverageCo2 = g.Where(sr => sr.Co2.HasValue).Average(sr => (double?)sr.Co2),
                    AveragePm25 = g.Where(sr => sr.Pm25.HasValue).Average(sr => (double?)sr.Pm25),
                    AverageHumidity = g.Where(sr => sr.Humidity.HasValue).Average(sr => (double?)sr.Humidity),
                    AverageEnergy = g.Where(sr => sr.Energy.HasValue).Average(sr => sr.Energy),
                    Count = g.Count()
                })
                .OrderBy(r => r.Timestamp);
        }
        else // Minute intervals
        {
            aggregated = query
                .GroupBy(sr => new
                {
                    Year = sr.Timestamp.Year,
                    Month = sr.Timestamp.Month,
                    Day = sr.Timestamp.Day,
                    Hour = sr.Timestamp.Hour,
                    Minute = sr.Timestamp.Minute / intervalMinutes * intervalMinutes
                })
                .Select(g => new AggregatedSensorReading
                {
                    Timestamp = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, g.Key.Minute, 0),
                    AverageCo2 = g.Where(sr => sr.Co2.HasValue).Average(sr => (double?)sr.Co2),
                    AveragePm25 = g.Where(sr => sr.Pm25.HasValue).Average(sr => (double?)sr.Pm25),
                    AverageHumidity = g.Where(sr => sr.Humidity.HasValue).Average(sr => (double?)sr.Humidity),
                    AverageEnergy = g.Where(sr => sr.Energy.HasValue).Average(sr => sr.Energy),
                    Count = g.Count()
                })
                .OrderBy(r => r.Timestamp);
        }
        
        var result = await aggregated.ToListAsync();
        logger.LogDebug("Aggregated {Count} intervals of {Interval} minutes", result.Count, intervalMinutes);
        return result;
    }
}
