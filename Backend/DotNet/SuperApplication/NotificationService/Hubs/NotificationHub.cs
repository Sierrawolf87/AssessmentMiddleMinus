using Microsoft.AspNetCore.SignalR;
using SuperApplication.Shared.Data.Entities;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    public async Task SendSensorReading(SensorReading reading)
    {
        await Clients.All.SendAsync("ReceiveSensorReading", reading);
    }
}