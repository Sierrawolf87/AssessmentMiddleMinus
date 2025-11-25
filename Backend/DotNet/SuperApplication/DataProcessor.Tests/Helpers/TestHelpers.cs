using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SuperApplication.Shared.Data;

namespace DataProcessor.Tests.Helpers;

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
    /// Creates a mock logger for testing
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a sample sensor message JSON with valid data
    /// </summary>
    public static string CreateValidSensorMessageJson()
    {
        return @"[
            {
                ""type"": ""air_quality"",
                ""name"": ""Living Room"",
                ""payload"": {
                    ""co2"": 450,
                    ""pm25"": 12,
                    ""humidity"": 55
                }
            },
            {
                ""type"": ""motion"",
                ""name"": ""Kitchen"",
                ""payload"": {
                    ""motionDetected"": true
                }
            },
            {
                ""type"": ""energy"",
                ""name"": ""Bedroom"",
                ""payload"": {
                    ""energy"": 1234.56
                }
            }
        ]";
    }

    /// <summary>
    /// Creates an invalid JSON string for testing error handling
    /// </summary>
    public static string CreateInvalidJson()
    {
        return "{invalid json structure without closing brace";
    }
}
