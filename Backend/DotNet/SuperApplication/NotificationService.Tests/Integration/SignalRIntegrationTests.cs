using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NotificationService.Hubs;
using NotificationService.Tests.Helpers;
using SuperApplication.Shared.Data.Entities;

namespace NotificationService.Tests.Integration;

/// <summary>
/// Integration tests for SignalR broadcasting functionality
/// These tests verify the NotificationHub can properly broadcast messages
/// </summary>
public class SignalRIntegrationTests
{
    [Fact]
    public async Task NotificationHub_ShouldBroadcastSensorReading()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var capturedMessage = default(SensorReading);

        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        mockClientProxy
            .Setup(proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.IsAny<object[]>(),
                default))
            .Callback<string, object[], System.Threading.CancellationToken>((method, args, ct) =>
            {
                capturedMessage = args[0] as SensorReading;
            })
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub
        {
            Clients = mockClients.Object
        };

        var testReading = TestHelpers.CreateSampleSensorReading();

        // Act
        await hub.SendSensorReading(testReading);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage.Should().BeSameAs(testReading);
        capturedMessage!.Id.Should().Be(testReading.Id);
        capturedMessage.Type.Should().Be(testReading.Type);
        capturedMessage.Name.Should().Be(testReading.Name);
    }

    [Fact]
    public async Task NotificationHub_ShouldBroadcastMultipleReadings()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var broadcastCount = 0;

        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        mockClientProxy
            .Setup(proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.IsAny<object[]>(),
                default))
            .Callback(() => broadcastCount++)
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub
        {
            Clients = mockClients.Object
        };

        var readings = TestHelpers.CreateSampleSensorReadingList();

        // Act
        foreach (var reading in readings)
        {
            await hub.SendSensorReading(reading);
        }

        // Assert
        broadcastCount.Should().Be(readings.Count);
    }

    [Fact]
    public async Task NotificationHub_ShouldBroadcastToAllClients()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockAllClientsProxy = new Mock<IClientProxy>();

        mockClients.Setup(clients => clients.All).Returns(mockAllClientsProxy.Object);

        var hub = new NotificationHub
        {
            Clients = mockClients.Object
        };

        var testReading = TestHelpers.CreateSampleSensorReading();

        // Act
        await hub.SendSensorReading(testReading);

        // Assert - verify it was sent to All clients, not to specific clients
        mockClients.Verify(clients => clients.All, Times.Once);
        mockAllClientsProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotificationHub_ShouldPreserveMessageData()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        SensorReading? receivedReading = null;

        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        mockClientProxy
            .Setup(proxy => proxy.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .Callback<string, object[], System.Threading.CancellationToken>((_, args, _) =>
            {
                receivedReading = args[0] as SensorReading;
            })
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub
        {
            Clients = mockClients.Object
        };

        var originalReading = new SensorReading
        {
            Id = Guid.NewGuid(),
            Type = SuperApplication.Shared.Data.Entities.Enums.SensorType.AirQuality,
            Name = SuperApplication.Shared.Data.Entities.Enums.SensorLocation.Kitchen,
            Timestamp = DateTime.UtcNow,
            Co2 = 500,
            Pm25 = 15,
            Humidity = 60
        };

        // Act
        await hub.SendSensorReading(originalReading);

        // Assert
        receivedReading.Should().NotBeNull();
        receivedReading!.Id.Should().Be(originalReading.Id);
        receivedReading.Co2.Should().Be(500);
        receivedReading.Pm25.Should().Be(15);
        receivedReading.Humidity.Should().Be(60);
    }
}
