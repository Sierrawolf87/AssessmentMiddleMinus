using NotificationService.Hubs;
using NotificationService.Services;
using NotificationService.Configuration;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var lokiUrl = builder.Configuration["Logging:LokiPath"] ?? "http://loki:3100";
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(lokiUrl, labels: new[] { new LokiLabel { Key = "service", Value = "NotificationService" } })
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // Allow any origin for development
    });
});

builder.Services.AddSignalR();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<RabbitMqListener>();

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment() && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.Run();

// Ensure logs are flushed on shutdown
Log.CloseAndFlush();