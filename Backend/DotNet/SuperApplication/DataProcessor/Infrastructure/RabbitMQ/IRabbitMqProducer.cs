namespace DataProcessor.Infrastructure.RabbitMQ;

public interface IRabbitMqProducer
{
    Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default);
}
