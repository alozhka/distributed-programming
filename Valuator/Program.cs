using Valuator.Hubs;
using Valuator.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

await builder.Services.AddServices();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<RankHub>("/rank");

app.Run();