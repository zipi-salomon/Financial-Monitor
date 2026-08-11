using FinancialMonitor.Backend.Data;
using FinancialMonitor.Backend.Repositories;
using FinancialMonitor.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var clientUrl = builder.Configuration["FrontendSettings:ClientUrl"] ?? "http://localhost:3000";
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddControllers();

// Redis Connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(redisConnectionString);
    config.ConnectTimeout = 3000;
    config.SyncTimeout = 3000;
    config.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(config);
});

// Real-Time & Database
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services
builder.Services.AddScoped<SqlTransactionRepository>();
builder.Services.AddScoped<ITransactionRepository>(sp =>
{
    var sqlRepo = sp.GetRequiredService<SqlTransactionRepository>();
    var redis = sp.GetRequiredService<IConnectionMultiplexer>();
    return ActivatorUtilities.CreateInstance<CachedTransactionRepository>(sp, sqlRepo, redis);
});
builder.Services.AddScoped<ITransactionService, TransactionService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(clientUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();
app.MapHub<FinancialMonitor.Backend.Hubs.TransactionHub>("/hubs/transactions");

app.Run();