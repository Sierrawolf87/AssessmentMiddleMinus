using System.Text;
using DataProcessor.Features.Messages;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataProcessor.Infrastructure.RabbitMQ;

public class RabbitMqConsumerService(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<RabbitMqConsumerService> logger)
    : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;

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
            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            // Ensure queue exists
            await _channel.QueueDeclareAsync(queue: _options.DataQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                await ProcessMessageAsync(message, routingKey);
            };

            await _channel.BasicConsumeAsync(queue: _options.DataQueueName, autoAck: true, consumer: consumer, cancellationToken: ct);
            
            // Keep the service running indefinitely
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting RabbitMQ consumer");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string message, string routingKey)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            await mediator.Send(new ProcessMessageCommand(message, routingKey));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message");
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
