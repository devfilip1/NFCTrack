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
using ShortLink.Infrastructure.Identity;
using ShortLink.Application.Auth;
using StackExchange.Redis;
using Polly;
using ShortLink.Infrastructure.Resilience;
using Polly.CircuitBreaker;
using Polly.Retry;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
            options.ConfigurationOptions = ConfigurationOptions.Parse(connectionStringRedis);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 1000;
            options.ConfigurationOptions.ConnectRetry = 1;
            options.ConfigurationOptions.AsyncTimeout = 1000;
            options.InstanceName = "shortlink:";
        });

        // Only a circuit breaker: the Redis client ignores cancellation while connecting, so a
        // Polly timeout would not shorten the wait. Its own ConnectTimeout/AsyncTimeout do that.
        services.AddResiliencePipeline(ResiliencePipelines.RedisCache, pipeline =>
            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
            }));

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

            // Redirects keep working without RabbitMQ (only clicks are lost), so it is "degraded", not "unhealthy".
            x.ConfigureHealthCheckOptions(options => options.MinimalFailureStatus = HealthStatus.Degraded);

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

        // Outermost first: total timeout -> retry -> circuit breaker -> per-attempt timeout.
        services.AddResiliencePipeline(ResiliencePipelines.ClickPublish, pipeline => pipeline
            .AddTimeout(TimeSpan.FromSeconds(2))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
            })
            // The defaults need 100 calls before the circuit can open; a redirect makes up to 3 attempts.
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
            })
            .AddTimeout(TimeSpan.FromMilliseconds(800)));

        services.AddScoped<IClickTracker, MassTransitClickTracker>();

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.SigningKey.Length >= 32, "Jwt:SigningKey must be at least 32 characters.")
            .Validate(o => o.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IAuthService, IdentityAuthService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringRedis = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");
        var rabbitMqHost = configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("Configuration 'RabbitMq:Host' was not found.");

        // Fail fast: by default the client retries the connection 3 x 5s, which would exceed the
        // container healthcheck timeout and mark the whole API unhealthy just because Redis is down.
        var redisHealthOptions = ConfigurationOptions.Parse(connectionStringRedis);
        redisHealthOptions.ConnectTimeout = 1000;
        redisHealthOptions.ConnectRetry = 1;

        // SQL Server is required to serve anything (unhealthy if down). Redis is only a cache and
        // RabbitMQ only carries clicks, so redirects keep working without them (degraded).
        // MassTransit also registers its own "masstransit-bus" check (see AddMessaging).
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "sqlserver", tags: ["ready"])
            .AddRedis(redisHealthOptions.ToString(), name: "redis", failureStatus: HealthStatus.Degraded, tags: ["ready"])
            .AddCheck("rabbitmq", new RabbitMqHealthCheck(rabbitMqHost, 5672), HealthStatus.Degraded, ["ready"]);

        return services;
    }
}
