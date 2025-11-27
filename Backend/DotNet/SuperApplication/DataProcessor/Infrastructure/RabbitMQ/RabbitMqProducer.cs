using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DataProcessor.Infrastructure.RabbitMQ;

public class RabbitMqProducer(IOptions<RabbitMqOptions> options, ILogger<RabbitMqProducer> logger) : IRabbitMqProducer, IDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            if (int.TryParse(_options.Port, out var port))
            {
                factory.Port = port;
            }

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            logger.LogInformation("Successfully connected to RabbitMQ at {HostName}:{Port}", _options.HostName, _options.Port);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create RabbitMQ connection/channel");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionAsync(cancellationToken);

        if (_channel == null) throw new InvalidOperationException("RabbitMQ channel is not initialized");

        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, mandatory: false, basicProperties: properties, body: body, cancellationToken: cancellationToken);
        logger.LogDebug("Published message to queue {QueueName}. Size: {Size} bytes", queueName, body.Length);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _lock.Dispose();
    }
}
