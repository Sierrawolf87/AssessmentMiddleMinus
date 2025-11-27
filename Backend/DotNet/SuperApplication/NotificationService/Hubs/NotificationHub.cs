using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SuperApplication.Shared.Data.Entities;

namespace NotificationService.Hubs;

public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(exception, "Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendSensorReading(SensorReading reading)
    {
        await Clients.All.SendAsync("ReceiveSensorReading", reading);
    }
}