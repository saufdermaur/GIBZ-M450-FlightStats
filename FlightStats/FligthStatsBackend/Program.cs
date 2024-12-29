using Backend;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHangfire(config =>
//    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//          .UseSimpleAssemblyNameTypeSerializer()
//          .UseDefaultTypeSerializer()
//          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
//              new SqlServerStorageOptions
//              {
//                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
//                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
//                  QueuePollInterval = TimeSpan.Zero,
//                  UseRecommendedIsolationLevel = true,
//                  DisableGlobalLocks = true
//              }));

//builder.Services.AddHangfireServer();

builder.Services.AddDbContext<FlightStatsDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 1, 0))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
