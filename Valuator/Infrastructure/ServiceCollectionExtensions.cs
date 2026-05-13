using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Auth;
using Valuator.Services;

namespace Valuator.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static async Task AddServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddScoped<TextRepository>();
        services.AddScoped<ValuatorService>();
        services.AddScoped<EventPublisher>();

        services.AddScoped<UserRepository>();
        services.AddScoped<AuthService>();
        services.AddSingleton<PasswordHasher>();

        services.AddRedis(cfg);
        services.AddCookieAuth();
        await services.AddRabbitMq(cfg);
    }

    private static void AddCookieAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.Name)
                .Build());
    }

    private static async Task AddRabbitMq(this IServiceCollection services, IConfiguration cfg)
    {
        string rabbitConnectionString = cfg.GetConnectionString("RabbitMq")!;
        ConnectionFactory rabbitmqConnectionFactory = new ConnectionFactory
        {
            Uri = new Uri(rabbitConnectionString),
        };

        IConnection rabbitmqConnection = await rabbitmqConnectionFactory.CreateConnectionAsync();
        IChannel rabbitmqChannel = await rabbitmqConnection.CreateChannelAsync();
        await DeclareTypology(rabbitmqChannel);

        services.AddSingleton(rabbitmqChannel);
    }

    private static async Task DeclareTypology(IChannel rabbitmqChannel)
    {
        await rabbitmqChannel.ExchangeDeclareAsync(
            EventPublisher.RankExchangeName,
            ExchangeType.Direct
        );
        await rabbitmqChannel.QueueDeclareAsync(
            EventPublisher.RankQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await rabbitmqChannel.QueueBindAsync(
            EventPublisher.RankQueueName,
            EventPublisher.RankExchangeName,
            routingKey: ""
        );

        await rabbitmqChannel.ExchangeDeclareAsync(
            EventPublisher.SimilarityExchangeName,
            ExchangeType.Fanout
        );
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration cfg)
    {
        string redisConnectionString = cfg.GetConnectionString("Redis")!;
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
            .SetApplicationName("Valuator");
    }
}