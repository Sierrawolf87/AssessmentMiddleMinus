using SuperApplication.Shared.Data;
using DataProcessor.Infrastructure.RabbitMQ;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var lokiUrl = builder.Configuration["Logging:LokiPath"] ?? "http://loki:3100";
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(lokiUrl, labels: new[] { new LokiLabel { Key = "service", Value = "DataProcessor" } })
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database migration");
        throw;
    }
}

app.MapHealthChecks("/health");

app.Run();

// Ensure logs are flushed on shutdown
Log.CloseAndFlush();