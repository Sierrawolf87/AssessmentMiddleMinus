using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SuperApplication.Shared.Data.Entities;
using NotificationService.Configuration;
using NotificationService.Hubs;

namespace NotificationService.Services;

public class RabbitMqListener : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RabbitMqListener> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqListener(IOptions<RabbitMqOptions> options, IHubContext<NotificationHub> hubContext,
        ILogger<RabbitMqListener> logger)
    {
        _options = options.Value;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
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

        try
        {
            _logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}", _options.HostName, _options.Port);
            
            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _channel.QueueDeclareAsync(queue: _options.NotificationQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var sensorReading = JsonSerializer.Deserialize<List<SensorReading>>(content);

                    if (sensorReading != null)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveSensorReading", sensorReading, ct);
                        _logger.LogInformation("Broadcasted {Count} sensor readings to SignalR clients", sensorReading.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
                
                await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            };

            await _channel.BasicConsumeAsync(queue: _options.NotificationQueueName, autoAck: false, consumer: consumer,
                cancellationToken: ct);

            _logger.LogInformation("RabbitMQ listener started successfully. Listening on queue: {QueueName}", _options.NotificationQueueName);

            // Keep the service running
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // This is expected when the application is shutting down
            _logger.LogInformation("RabbitMQ listener is shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting RabbitMQ listener");
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}