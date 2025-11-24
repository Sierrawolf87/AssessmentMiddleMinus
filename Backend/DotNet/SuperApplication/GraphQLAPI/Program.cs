using GraphQLAPI.Queries;
using Microsoft.EntityFrameworkCore;
using SuperApplication.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

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

// Add GraphQL server with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<SensorReadingQueries>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyPagingOptions(options =>
    {
        options.MaxPageSize = 100;
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

app.Run();