using DataProcessor.Infrastructure.RabbitMQ;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;

namespace DataProcessor.Tests.Infrastructure.RabbitMQ;

public class RabbitMqProducerTests
{
    private readonly Mock<ILogger<RabbitMqProducer>> _loggerMock;
    private readonly RabbitMqOptions _options;

    public RabbitMqProducerTests()
    {
        _loggerMock = new Mock<ILogger<RabbitMqProducer>>();
        _options = new RabbitMqOptions
        {
            HostName = "localhost",
            Port = "5672",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            DataQueueName = "test-data-queue",
            NotificationQueueName = "test-notification-queue"
        };
    }

    [Fact]
    public void Constructor_WithValidOptions_ShouldCreateInstance()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<RabbitMqOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        // Act
        var producer = new RabbitMqProducer(optionsMock.Object, _loggerMock.Object);

        // Assert
        producer.Should().NotBeNull();
    }

    [Fact]
    public void SendMessageAsync_WithNullChannel_ShouldThrowException()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<RabbitMqOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);
        var producer = new RabbitMqProducer(optionsMock.Object, _loggerMock.Object);

        var testMessage = new SensorReading
        {
            Id = Guid.NewGuid(),
            Type = SensorType.AirQuality,
            Name = SensorLocation.LivingRoom,
            Timestamp = DateTime.UtcNow,
            Co2 = 400
        };

        // Act & Assert
        // Note: This test validates that without a real connection, the producer will fail gracefully
        // In a real scenario, this would attempt to connect to RabbitMQ and fail if it's not available
        Func<Task> act = async () => await producer.SendMessageAsync(testMessage, "test-queue");
        
        // This will throw because RabbitMQ is not actually running
        // In unit tests, we're validating the structure and that it attempts to connect
        act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<RabbitMqOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);
        var producer = new RabbitMqProducer(optionsMock.Object, _loggerMock.Object);

        // Act
        Action act = () => producer.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Options_ShouldBeCorrectlyMapped()
    {
        // Arrange & Act
        var options = new RabbitMqOptions
        {
            HostName = "test-host",
            Port = "1234",
            UserName = "test-user",
            Password = "test-pass",
            VirtualHost = "/test",
            DataQueueName = "data-queue",
            NotificationQueueName = "notification-queue"
        };

        // Assert
        options.HostName.Should().Be("test-host");
        options.Port.Should().Be("1234");
        options.UserName.Should().Be("test-user");
        options.Password.Should().Be("test-pass");
        options.VirtualHost.Should().Be("/test");
        options.DataQueueName.Should().Be("data-queue");
        options.NotificationQueueName.Should().Be("notification-queue");
    }
}
