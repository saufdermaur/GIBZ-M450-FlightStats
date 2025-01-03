using Backend;
using Backend.Selenium;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(configuration => configuration
     .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
     .UseSimpleAssemblyNameTypeSerializer()
     .UseRecommendedSerializerSettings()
     .UseStorage(new MySqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlStorageOptions() { TablesPrefix = "Hangfire" })));

builder.Services.AddHangfireServer();

builder.Services.AddDbContext<FlightStatsDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 1, 0))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISeleniumFlights, SeleniumFlights>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
