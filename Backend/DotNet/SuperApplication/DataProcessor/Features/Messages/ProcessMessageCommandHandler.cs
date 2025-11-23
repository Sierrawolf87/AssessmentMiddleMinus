using System.Text.Json;
using SuperApplication.Shared.Data;
using SuperApplication.Shared.Data.Entities;
using SuperApplication.Shared.Models;
using DataProcessor.Infrastructure.RabbitMQ;
using MediatR;
using Microsoft.Extensions.Options;

namespace DataProcessor.Features.Messages;

public class ProcessMessageCommandHandler(
    ApplicationDbContext dbContext,
    ILogger<ProcessMessageCommandHandler> logger,
    IRabbitMqProducer producer,
    IOptions<RabbitMqOptions> options)
    : IRequestHandler<ProcessMessageCommand>
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task Handle(ProcessMessageCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var sensorMessages = JsonSerializer.Deserialize<List<SensorMessageDto>>(request.Content, options);

            if (sensorMessages != null)
            {
                foreach (var msg in sensorMessages)
                {
                    var reading = new SensorReading
                    {
                        Id = Guid.NewGuid(),
                        Type = msg.Type,
                        Name = msg.Name,
                        Timestamp = now,
                        Co2 = msg.Payload?.Co2,
                        Pm25 = msg.Payload?.Pm25,
                        Humidity = msg.Payload?.Humidity,
                        MotionDetected = msg.Payload?.MotionDetected,
                        Energy = msg.Payload?.Energy
                    };
                    dbContext.SensorReadings.Add(reading);
                    
                    // Send notification
                    await producer.SendMessageAsync(reading, _options.NotificationQueueName, cancellationToken);
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize message content. Raw content saved.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
