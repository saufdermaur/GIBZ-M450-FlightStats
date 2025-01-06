using Backend;
using Backend.Selenium;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

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
builder.Services.AddScoped<IWebDriver>(serviceProvider =>
{
    FirefoxOptions options = new FirefoxOptions();
    FirefoxProfile profile = new FirefoxProfile();
    profile.SetPreference("intl.accept_languages", "de");
    options.Profile = profile;

    //options.AddArgument("--headless");

    return new FirefoxDriver(options);
});


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
