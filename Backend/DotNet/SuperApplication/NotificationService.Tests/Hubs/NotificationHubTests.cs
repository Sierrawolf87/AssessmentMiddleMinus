using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Hubs;
using NotificationService.Tests.Helpers;
using SuperApplication.Shared.Data.Entities;

namespace NotificationService.Tests.Hubs;

public class NotificationHubTests
{
    [Fact]
    public async Task SendSensorReading_ShouldBroadcastToAllClients()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        var hub = new NotificationHub(NullLogger<NotificationHub>.Instance)
        {
            Clients = mockClients.Object
        };

        var sensorReading = TestHelpers.CreateSampleSensorReading();

        // Act
        await hub.SendSensorReading(sensorReading);

        // Assert
        mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.Is<object[]>(args => args.Length == 1 && args[0] == sensorReading),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendSensorReading_WithNullReading_ShouldStillBroadcast()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        var hub = new NotificationHub(NullLogger<NotificationHub>.Instance)
        {
            Clients = mockClients.Object
        };

        // Act
        await hub.SendSensorReading(null!);

        // Assert
        mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.Is<object[]>(args => args.Length == 1 && args[0] == null),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendSensorReading_ShouldUseCorrectMethodName()
    {
        // Arrange
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
        
        var hub = new NotificationHub(NullLogger<NotificationHub>.Instance)
        {
            Clients = mockClients.Object
        };

        var sensorReading = TestHelpers.CreateSampleSensorReading();

        // Act
        await hub.SendSensorReading(sensorReading);

        // Assert - verify the method name is exactly "ReceiveSensorReading"
        mockClientProxy.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceiveSensorReading",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }
}
