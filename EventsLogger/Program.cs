using EventsLogger.Infrastructure;
using EventsLogger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

await builder.Services.AddServices(builder.Configuration);
builder.Services.AddHostedService<EventsConsumer>();

IHost app = builder.Build();

app.Run();
