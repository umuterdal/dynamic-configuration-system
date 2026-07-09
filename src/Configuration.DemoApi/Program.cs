using Configuration.Application.Extensions;
using Configuration.Domain.Entities;
using Configuration.Infrastructure.Extensions;
using Configuration.Library.Extensions;
using Configuration.Library.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/demoapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Configuration Demo API", Version = "v1" });
});

// Add Infrastructure services (MongoDB)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services
builder.Services.AddApplicationServices();

// Add ConfigurationReader
builder.Services.AddConfigurationReader(builder.Configuration);

// Add BackgroundService for periodic refresh
builder.Services.AddHostedService<ConfigurationRefreshService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
