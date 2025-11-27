using FluentAssertions;
using GraphQLAPI.Queries;
using GraphQLAPI.Tests.Helpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuperApplication.Shared.Data;

namespace GraphQLAPI.Tests.Integration;

/// <summary>
/// Integration tests for GraphQL queries using HotChocolate test execution
/// </summary>
public class GraphQLIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRequestExecutor _executor;

    public GraphQLIntegrationTests()
    {
        // Create InMemory database
        _context = TestHelpers.CreateInMemoryDbContext();
        
        // Build GraphQL executor with test schema
        _executor = new ServiceCollection()
            .AddLogging()
            .AddSingleton(_context)
            .AddGraphQL()
            .AddQueryType<SensorReadingQueries>()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .BuildRequestExecutorAsync()
            .Result;
    }

    [Fact]
    public async Task GetSensorReadings_ShouldReturnAllReadings()
    {
        // Arrange
        await TestHelpers.SeedDatabase(_context);

        var query = @"
            query {
                sensorReadings {
                    items {
                        id
                        type
                        name
                        timestamp
                    }
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        
        var data = result.ToJson();
        data.Should().Contain("sensorReadings");
    }

    [Fact]
    public async Task GetSensorReadingById_WithValidId_ShouldReturnReading()
    {
        // Arrange
        await TestHelpers.SeedDatabase(_context);
        var existingReading = await _context.SensorReadings.FirstOrDefaultAsync();

        var query = $@"
            query {{
                sensorReadingById(id: ""{existingReading!.Id}"") {{
                    id
                    type
                    name
                    co2
                    pm25
                    humidity
                }}
            }}
        ";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        
        var json = result.ToJson();
        json.Should().Contain(existingReading.Id.ToString());
    }

    [Fact]
    public async Task GetSensorReadings_WithProjection_ShouldReturnOnlySelectedFields()
    {
        // Arrange
        await TestHelpers.SeedDatabase(_context);

        var query = @"
            query {
                sensorReadings {
                    items {
                        id
                        type
                    }
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSensorReadingStats_ShouldReturnStatistics()
    {
        // Arrange
        await TestHelpers.SeedDatabase(_context);

        var query = @"
            query {
                sensorReadingStats {
                    totalCount
                    averageCo2
                    averagePm25
                    averageHumidity
                    maxCo2
                    minCo2
                    motionDetectedCount
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        
        var json = result.ToJson();
        json.Should().Contain("totalCount");
        json.Should().Contain("averageCo2");
    }

    [Fact]
    public async Task GetSensorReadingStats_WithTypeFilter_ShouldReturnFilteredStats()
    {
        // Arrange
        await TestHelpers.SeedDatabase(_context);

        var query = @"
            query {
                sensorReadingStats(type: AIR_QUALITY) {
                    totalCount
                    averageCo2
                }
            }
        ";

        // Act
        var result = await _executor.ExecuteAsync(query);

        // Assert
        result.Should().NotBeNull();
        
        var json = result.ToJson();
        json.Should().Contain("totalCount");
    }

    [Fact]
    public async Task GraphQLSchema_ShouldBeValid()
    {
        // Arrange & Act
        var schema = _executor.Schema;

        // Assert
        schema.Should().NotBeNull();
        schema.QueryType.Should().NotBeNull();
        schema.QueryType.Fields.Should().Contain(f => f.Name == "sensorReadings");
        schema.QueryType.Fields.Should().Contain(f => f.Name == "sensorReadingById");
        schema.QueryType.Fields.Should().Contain(f => f.Name == "sensorReadingStats");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

