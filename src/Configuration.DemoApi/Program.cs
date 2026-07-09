using System.Reflection;
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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dynamic Configuration Demo API",
        Version = "v1",
        Description = "Demonstrates how to use the ConfigurationReader library to access dynamic configurations with type-safe values, caching, and automatic refresh.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Configuration System",
            Url = new Uri("https://github.com/example/configuration-system")
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add Infrastructure services (MongoDB)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services
builder.Services.AddApplicationServices();

// Add ConfigurationReader
builder.Services.AddConfigurationReader(builder.Configuration);

// Add BackgroundService for periodic refresh (polling — primary mechanism)
builder.Services.AddHostedService<ConfigurationRefreshService>();

// Add RabbitMQ broker consumer (improves refresh latency — secondary mechanism)
builder.Services.AddConfigurationBrokerConsumer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler("/error");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamic Configuration Demo API v1");
        c.DocumentTitle = "Configuration API Documentation";
        c.DefaultModelsExpandDepth(2);
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();
