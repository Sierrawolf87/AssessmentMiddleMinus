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
}
