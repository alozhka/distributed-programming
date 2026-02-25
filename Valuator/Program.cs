using StackExchange.Redis;
using Valuator.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddScoped<IConnectionMultiplexer, ConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect("redis:6379")
);
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