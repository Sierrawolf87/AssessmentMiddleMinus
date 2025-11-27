using FluentAssertions;
using GraphQLAPI.Queries;
using GraphQLAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using SuperApplication.Shared.Data.Entities.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace GraphQLAPI.Tests.Queries;

public class SensorReadingQueriesTests : IDisposable
{
    private readonly SensorReadingQueries _queries;
    private readonly ILogger<SensorReadingQueries> _logger;

    public SensorReadingQueriesTests()
    {
        _queries = new SensorReadingQueries();
        _logger = NullLogger<SensorReadingQueries>.Instance;
    }

    [Fact]
    public void GetSensorReadings_ShouldReturnQueryable()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();

        // Act
        // Act
        var result = _queries.GetSensorReadings(context, _logger);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IQueryable<SuperApplication.Shared.Data.Entities.SensorReading>>();
    }

    [Fact]
    public async Task GetSensorReadings_ShouldReturnAllReadings()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var result = _queries.GetSensorReadings(context, _logger);
        var readings = await result.ToListAsync();

        // Assert
        readings.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetSensorReadingById_WithExistingId_ShouldReturnReading()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        var sample = TestHelpers.CreateSampleReading();
        context.SensorReadings.Add(sample);
        await context.SaveChangesAsync();

        // Act
        // Act
        var result = await _queries.GetSensorReadingById(sample.Id, context, _logger);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sample.Id);
        result.Type.Should().Be(sample.Type);
        result.Name.Should().Be(sample.Name);
    }

    [Fact]
    public async Task GetSensorReadingById_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        var nonExistingId = Guid.NewGuid();

        // Act
        // Act
        var result = await _queries.GetSensorReadingById(nonExistingId, context, _logger);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSensorReadingStats_WithNoFilters_ShouldReturnAllStats()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().Be(5);
        stats.AverageCo2.Should().BeGreaterThan(0);
        stats.AveragePm25.Should().BeGreaterThan(0);
        stats.AverageHumidity.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSensorReadingStats_WithTypeFilter_ShouldReturnFilteredStats()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger, type: SensorType.AirQuality);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().Be(3); // Only 3 AirQuality sensors in seed data
    }

    [Fact]
    public async Task GetSensorReadingStats_WithLocationFilter_ShouldReturnFilteredStats()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger, name: SensorLocation.LivingRoom);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().Be(1); // Only 1 reading from LivingRoom
    }

    [Fact]
    public async Task GetSensorReadingStats_WithDateRange_ShouldReturnFilteredStats()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);
        var startDate = DateTime.UtcNow.AddHours(-3.5);
        var endDate = DateTime.UtcNow.AddHours(-1.5);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger, startDate: startDate, endDate: endDate);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalCount.Should().BeGreaterThan(0);
        stats.TotalCount.Should().BeLessThan(5);
    }

    [Fact]
    public async Task GetSensorReadingStats_ShouldCalculateCorrectAverages()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger);

        // Assert
        stats.AverageCo2.Should().NotBeNull();
        stats.AverageCo2.Should().Be((400 + 500 + 450) / 3.0); // Average of 3 CO2 values
        
        stats.AveragePm25.Should().NotBeNull();
        stats.AveragePm25.Should().Be((10 + 15 + 12) / 3.0); // Average of 3 PM2.5 values
        
        stats.AverageHumidity.Should().NotBeNull();
        stats.AverageHumidity.Should().Be((50 + 60 + 55) / 3.0); // Average of 3 humidity values
    }

    [Fact]
    public async Task GetSensorReadingStats_ShouldFindCorrectMinMax()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger);

        // Assert
        stats.MaxCo2.Should().Be(500);
        stats.MinCo2.Should().Be(400);
        stats.MaxPm25.Should().Be(15);
        stats.MinPm25.Should().Be(10);
    }

    [Fact]
    public async Task GetSensorReadingStats_ShouldCountMotionDetections()
    {
        // Arrange
        using var context = TestHelpers.CreateInMemoryDbContext();
        await TestHelpers.SeedDatabase(context);

        // Act
        // Act
        var stats = await _queries.GetSensorReadingStats(context, _logger);

        // Assert
        stats.MotionDetectedCount.Should().Be(1); // Only 1 motion sensor with true
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
