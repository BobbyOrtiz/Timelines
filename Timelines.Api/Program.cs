using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Timelines.Api.Services;
using Timelines.Data;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(Log.Logger);
});

// Get connection string with fallback logic
var connectionString = Environment.GetEnvironmentVariable("TIMELINES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("TimelinesDb")
    ?? "Server=localhost;Database=Timelines.Dev;Trusted_Connection=True;TrustServerCertificate=True;";

// Register DbContext
builder.Services.AddDbContext<TimelinesDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register DevUserContext
builder.Services.AddScoped<DevUserContext>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
