using Microsoft.EntityFrameworkCore;
using OrderWorker;
using OrderWorker.Data;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
    "Host=localhost;Port=5432;Database=carsale;Username=admin;Password=admin");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    return ConnectionMultiplexer.Connect(
        "localhost:6379");
});

var host = builder.Build();
host.Run();
