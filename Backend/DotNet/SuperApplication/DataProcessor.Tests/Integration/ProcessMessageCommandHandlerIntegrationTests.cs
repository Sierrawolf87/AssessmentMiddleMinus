using DataProcessor.Features.Messages;
using DataProcessor.Infrastructure.RabbitMQ;
using DataProcessor.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SuperApplication.Shared.Data;
using SuperApplication.Shared.Data.Entities.Enums;

namespace DataProcessor.Tests.Integration;

/// <summary>
/// Integration tests for ProcessMessageCommandHandler using InMemory database
/// These tests verify the complete flow of message processing and database persistence
/// </summary>
public class ProcessMessageCommandHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IRabbitMqProducer> _producerMock;
    private readonly ProcessMessageCommandHandler _handler;

    public ProcessMessageCommandHandlerIntegrationTests()
    {
        // Create a unique database for each test
        _dbContext = TestHelpers.CreateInMemoryDbContext();
        
        var loggerMock = TestHelpers.CreateMockLogger<ProcessMessageCommandHandler>();
        _producerMock = new Mock<IRabbitMqProducer>();
        
        var optionsMock = new Mock<IOptions<RabbitMqOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new RabbitMqOptions
        {
            NotificationQueueName = "integration-test-notification-queue"
        });

        _handler = new ProcessMessageCommandHandler(
            _dbContext,
            loggerMock.Object,
            _producerMock.Object,
            optionsMock.Object
        );
    }

    [Fact]
    public async Task EndToEnd_ProcessValidMessage_ShouldPersistToDatabase()
    {
        // Arrange
        var messageJson = @"[
            {
                ""type"": ""air_quality"",
                ""name"": ""Living Room"",
                ""payload"": {
                    ""co2"": 500,
                    ""pm25"": 15,
                    ""humidity"": 60
                }
            }
        ]";
        var command = new ProcessMessageCommand(messageJson, "test.routing");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify database persistence
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().HaveCount(1);

        var reading = readings.First();
        reading.Type.Should().Be(SensorType.AirQuality);
        reading.Name.Should().Be(SensorLocation.LivingRoom);
        reading.Co2.Should().Be(500);
        reading.Pm25.Should().Be(15);
        reading.Humidity.Should().Be(60);
        reading.Id.Should().NotBeEmpty();
        reading.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task EndToEnd_ProcessMultipleMessages_ShouldPersistAllToDatabase()
    {
        // Arrange
        var messageJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(messageJson, "test.routing");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().HaveCount(3);

        // Verify all types are present
        readings.Select(r => r.Type).Should().Contain(new[]
        {
            SensorType.AirQuality,
            SensorType.Motion,
            SensorType.Energy
        });

        // Verify all locations are present
        readings.Select(r => r.Name).Should().Contain(new[]
        {
            SensorLocation.LivingRoom,
            SensorLocation.Kitchen,
            SensorLocation.Bedroom
        });
    }

    [Fact]
    public async Task EndToEnd_ProcessMessage_ShouldPublishToRabbitMq()
    {
        // Arrange
        var messageJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(messageJson, "test.routing");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify RabbitMQ producer was called
        _producerMock.Verify(
            x => x.SendMessageAsync(
                It.IsAny<object>(),
                "integration-test-notification-queue",
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task EndToEnd_ProcessMultipleBatches_ShouldAccumulateInDatabase()
    {
        // Arrange
        var batch1 = @"[{""type"": ""air_quality"", ""name"": ""Living Room"", ""payload"": {""co2"": 400}}]";
        var batch2 = @"[{""type"": ""motion"", ""name"": ""Kitchen"", ""payload"": {""motionDetected"": true}}]";
        var batch3 = @"[{""type"": ""energy"", ""name"": ""Bedroom"", ""payload"": {""energy"": 100.0}}]";

        // Act
        await _handler.Handle(new ProcessMessageCommand(batch1, "test"), CancellationToken.None);
        await _handler.Handle(new ProcessMessageCommand(batch2, "test"), CancellationToken.None);
        await _handler.Handle(new ProcessMessageCommand(batch3, "test"), CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().HaveCount(3);
        
        // Verify each type is present
        readings.Should().ContainSingle(r => r.Type == SensorType.AirQuality);
        readings.Should().ContainSingle(r => r.Type == SensorType.Motion);
        readings.Should().ContainSingle(r => r.Type == SensorType.Energy);
    }

    [Fact]
    public async Task EndToEnd_InvalidJson_ShouldNotPersistAnything()
    {
        // Arrange
        var invalidJson = "{this is not valid json";
        var command = new ProcessMessageCommand(invalidJson, "test.routing");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var readings = await _dbContext.SensorReadings.ToListAsync();
        readings.Should().BeEmpty();
    }

    [Fact]
    public async Task EndToEnd_QueryPersistedData_ShouldSupportFiltering()
    {
        // Arrange
        var messageJson = TestHelpers.CreateValidSensorMessageJson();
        var command = new ProcessMessageCommand(messageJson, "test.routing");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Test EF Core query capabilities
        var airQualityReadings = await _dbContext.SensorReadings
            .Where(r => r.Type == SensorType.AirQuality)
            .ToListAsync();

        airQualityReadings.Should().HaveCount(1);
        airQualityReadings.First().Name.Should().Be(SensorLocation.LivingRoom);

        var kitchenReadings = await _dbContext.SensorReadings
            .Where(r => r.Name == SensorLocation.Kitchen)
            .ToListAsync();

        kitchenReadings.Should().HaveCount(1);
        kitchenReadings.First().Type.Should().Be(SensorType.Motion);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
