using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RankCalculator.Infrastructure;
using RankCalculator.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

await builder.Services.AddServices(builder.Configuration);
builder.Services.AddHostedService<ValuatorConsumer>();

IHost app = builder.Build();

app.Run();