using DataProcessor.Features.Messages;
using DataProcessor.Infrastructure.RabbitMQ;
using DataProcessor.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SuperApplication.Shared.Data;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Data.Entities.Enums;

namespace DataProcessor.Tests.Features.Messages;

public class ProcessMessageCommandHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<ProcessMessageCommandHandler>> _loggerMock;
    private readonly Mock<IRabbitMqProducer> _producerMock;
    private readonly Mock<IOptions<RabbitMqOptions>> _optionsMock;
    private readonly ProcessMessageCommandHandler _handler;

    public ProcessMessageCommandHandlerTests()
    {
        _dbContext = TestHelpers.CreateInMemoryDbContext();
        _loggerMock = TestHelpers.CreateMockLogger<ProcessMessageCommandHandler>();
        _producerMock = new Mock<IRabbitMqProducer>();
        _optionsMock = new Mock<IOptions<RabbitMqOptions>>();

        var options = new RabbitMqOptions
        {
            NotificationQueueName = "test-notification-queue"
        };
        _optionsMock.Setup(x => x.Value).Returns(options);

        _handler = new ProcessMessageCommandHandler(
            _dbContext,
            _loggerMock.Object,
            _producerMock.Object,
            _optionsMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidJson_ShouldProcessAndSaveMessages()
    {
        // Arrange
        var validJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(validJson, "test.routing.key");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().HaveCount(3);

        // Verify first reading (Air Quality)
        var airQuality = readings.First(r => r.Type == SensorType.AirQuality);
        airQuality.Name.Should().Be(SensorLocation.LivingRoom);
        airQuality.Co2.Should().Be(450);
        airQuality.Pm25.Should().Be(12);
        airQuality.Humidity.Should().Be(55);
        airQuality.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify second reading (Motion)
        var motion = readings.First(r => r.Type == SensorType.Motion);
        motion.Name.Should().Be(SensorLocation.Kitchen);
        motion.MotionDetected.Should().Be(true);

        // Verify third reading (Energy)
        var energy = readings.First(r => r.Type == SensorType.Energy);
        energy.Name.Should().Be(SensorLocation.Bedroom);
        energy.Energy.Should().Be(1234.56);
    }

    [Fact]
    public async Task Handle_WithValidJson_ShouldCallRabbitMqProducer()
    {
        // Arrange
        var validJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(validJson, "test.routing.key");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _producerMock.Verify(
            x => x.SendMessageAsync(
                It.Is<List<SensorReading>>(list => list.Count == 3),
                "test-notification-queue",
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithInvalidJson_ShouldLogErrorAndContinue()
    {
        // Arrange
        var invalidJson = TestHelpers.CreateInvalidJson();
        var command = new ProcessMessageCommand(invalidJson, "test.routing.key");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().BeEmpty();

        // Verify that error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithEmptyArray_ShouldNotSaveAnything()
    {
        // Arrange
        var emptyArrayJson = "[]";
        var command = new ProcessMessageCommand(emptyArrayJson, "test.routing.key");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueIdsForEachReading()
    {
        // Arrange
        var validJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(validJson, "test.routing.key");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        var uniqueIds = readings.Select(r => r.Id).Distinct().ToList();
        uniqueIds.Should().HaveCount(readings.Count);
    }

    [Fact]
    public async Task Handle_ShouldSetSameTimestampForAllReadings()
    {
        // Arrange
        var validJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(validJson, "test.routing.key");

        // Act
        var beforeExecution = DateTime.UtcNow;
        await _handler.Handle(command, CancellationToken.None);
        var afterExecution = DateTime.UtcNow;

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().AllSatisfy(reading =>
        {
            reading.Timestamp.Should().BeOnOrAfter(beforeExecution);
            reading.Timestamp.Should().BeOnOrBefore(afterExecution);
        });

        // All timestamps should be within a few milliseconds of each other
        var timestamps = readings.Select(r => r.Timestamp).ToList();
        var maxDifference = timestamps.Max() - timestamps.Min();
        maxDifference.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }
}
