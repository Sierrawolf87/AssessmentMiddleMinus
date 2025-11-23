namespace DataProcessor.Infrastructure.RabbitMQ;

public class RabbitMqOptions
{
    public string HostName { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = string.Empty;
    public string DataQueueName { get; set; } = string.Empty;
    public string NotificationQueueName { get; set; } = string.Empty;
}
