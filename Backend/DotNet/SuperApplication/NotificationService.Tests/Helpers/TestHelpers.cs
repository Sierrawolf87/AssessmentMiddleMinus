using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;

namespace NotificationService.Tests.Helpers;

public static class TestHelpers
{
    /// <summary>
    /// Creates a mock IHubContext for testing
    /// </summary>
    public static Mock<IHubContext<T>> CreateMockHubContext<T>() where T : Hub
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        var mockHubContext = new Mock<IHubContext<T>>();
        mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
        
        return mockHubContext;
    }

    /// <summary>
    /// Creates a mock logger for testing
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a sample SensorReading for testing
    /// </summary>
    public static SensorReading CreateSampleSensorReading()
    {
        return new SensorReading
        {
            Id = Guid.NewGuid(),
            Type = SensorType.AirQuality,
            Name = SensorLocation.LivingRoom,
            Timestamp = DateTime.UtcNow,
            Co2 = 450,
            Pm25 = 12,
            Humidity = 55
        };
    }

    /// <summary>
    /// Creates a list of sample SensorReading objects
    /// </summary>
    public static List<SensorReading> CreateSampleSensorReadingList()
    {
        return new List<SensorReading>
        {
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.AirQuality,
                Name = SensorLocation.LivingRoom,
                Timestamp = DateTime.UtcNow,
                Co2 = 450,
                Pm25 = 12,
                Humidity = 55
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.Motion,
                Name = SensorLocation.Kitchen,
                Timestamp = DateTime.UtcNow,
                MotionDetected = true
            },
            new SensorReading
            {
                Id = Guid.NewGuid(),
                Type = SensorType.Energy,
                Name = SensorLocation.Bedroom,
                Timestamp = DateTime.UtcNow,
                Energy = 1234.56
            }
        };
    }

    /// <summary>
    /// Creates a JSON string representing a list of SensorReadings
    /// </summary>
    public static string CreateSensorReadingJson()
    {
        return @"[
            {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""type"": 0,
                ""name"": 3,
                ""co2"": 450,
                ""pm25"": 12,
                ""humidity"": 55,
                ""motionDetected"": null,
                ""energy"": null,
                ""timestamp"": ""2024-01-01T00:00:00Z""
            }
        ]";
    }
}
