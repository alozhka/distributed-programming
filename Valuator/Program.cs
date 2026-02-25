using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using Valuator.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("Valuator");

builder.Services.AddRazorPages();

builder.Services.AddScoped<TextRepository>();
builder.Services.AddScoped<ValuatorService>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();