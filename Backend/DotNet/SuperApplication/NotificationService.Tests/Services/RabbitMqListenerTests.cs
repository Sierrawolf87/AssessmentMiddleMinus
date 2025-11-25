using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotificationService.Configuration;
using NotificationService.Hubs;
using NotificationService.Services;
using NotificationService.Tests.Helpers;

namespace NotificationService.Tests.Services;

public class RabbitMqListenerTests
{
    private readonly Mock<IOptions<RabbitMqOptions>> _optionsMock;
    private readonly Mock<Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub>> _hubContextMock;
    private readonly Mock<ILogger<RabbitMqListener>> _loggerMock;

    public RabbitMqListenerTests()
    {
        _optionsMock = new Mock<IOptions<RabbitMqOptions>>();
        _hubContextMock = TestHelpers.CreateMockHubContext<NotificationHub>();
        _loggerMock = TestHelpers.CreateMockLogger<RabbitMqListener>();

        var options = new RabbitMqOptions
        {
            HostName = "localhost",
            Port = "5672",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            NotificationQueueName = "test-notification-queue"
        };

        _optionsMock.Setup(x => x.Value).Returns(options);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var listener = new RabbitMqListener(
            _optionsMock.Object,
            _hubContextMock.Object,
            _loggerMock.Object
        );

        // Assert
        listener.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldAcceptAllRequiredDependencies()
    {
        // Arrange & Act
        Action act = () => new RabbitMqListener(
            _optionsMock.Object,
            _hubContextMock.Object,
            _loggerMock.Object
        );

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var listener = new RabbitMqListener(
            _optionsMock.Object,
            _hubContextMock.Object,
            _loggerMock.Object
        );

        // Act
        Action act = () => listener.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RabbitMqOptions_ShouldBeCorrectlyMapped()
    {
        // Arrange
        var options = new RabbitMqOptions
        {
            HostName = "test-host",
            Port = "1234",
            UserName = "test-user",
            Password = "test-password",
            VirtualHost = "/test",
            NotificationQueueName = "test-queue"
        };

        // Assert
        options.HostName.Should().Be("test-host");
        options.Port.Should().Be("1234");
        options.UserName.Should().Be("test-user");
        options.Password.Should().Be("test-password");
        options.VirtualHost.Should().Be("/test");
        options.NotificationQueueName.Should().Be("test-queue");
    }

    [Fact]
    public void RabbitMqOptions_ShouldHaveDefaultEmptyStrings()
    {
        // Arrange & Act
        var options = new RabbitMqOptions();

        // Assert
        options.HostName.Should().Be(string.Empty);
        options.Port.Should().Be(string.Empty);
        options.UserName.Should().Be(string.Empty);
        options.Password.Should().Be(string.Empty);
        options.VirtualHost.Should().Be(string.Empty);
        options.NotificationQueueName.Should().Be(string.Empty);
    }
}
