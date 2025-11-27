using GraphQLAPI.Queries;
using Microsoft.EntityFrameworkCore;
using SuperApplication.Shared.Data;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var lokiUrl = builder.Configuration["Logging:LokiPath"] ?? "http://loki:3100";
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(lokiUrl, labels: new[] { new LokiLabel { Key = "service", Value = "GraphQLAPI" } })
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Add database context with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Add CORS for frontend access
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

// Add REST API controllers
builder.Services.AddControllers();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Add GraphQL server with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<SensorReadingQueries>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyPagingOptions(options =>
    {
        options.MaxPageSize = 200;
        options.DefaultPageSize = 20;
        options.IncludeTotalCount = true;
    })
    .ModifyCostOptions(options => options.MaxFieldCost = 10000);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable Banana Cake Pop (GraphQL IDE)
    app.UseRouting();
}

app.UseCors();

// Map GraphQL endpoint
app.MapGraphQL();

// Map REST API controllers
app.MapControllers();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.Run();

// Ensure logs are flushed on shutdown
Log.CloseAndFlush();