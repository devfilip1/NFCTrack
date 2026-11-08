using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShortLink.Application.Links;
using ShortLink.Infrastructure.Persistence;
using ShortLink.Infrastructure.Persistence.Repositories;
using ShortLink.Infrastructure.Caching;
using ShortLink.Application.Clicks;
using MassTransit;
using ShortLink.Infrastructure.Messaging;

namespace ShortLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Connection string 'Database' was not found.");
        var connectionStringRedis = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionStringRedis;
            options.InstanceName = "shortlink:";
        });

        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<IClickRepository, ClickRepository>();
        services.AddSingleton<ILinkCache, RedisLinkCache>();

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var host = configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("Configuration 'RabbitMq:Host' was not found.");
        var username = configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("Configuration 'RabbitMq:Username' was not found.");
        var password = configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("Configuration 'RabbitMq:Password' was not found.");

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IClickTracker, MassTransitClickTracker>();

        return services;
    }
}
