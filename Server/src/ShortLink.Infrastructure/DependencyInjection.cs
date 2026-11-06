using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShortLink.Application.Links;
using ShortLink.Infrastructure.Persistence;
using ShortLink.Infrastructure.Persistence.Repositories;
using ShortLink.Infrastructure.Caching;

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
        services.AddSingleton<ILinkCache, RedisLinkCache>();

        return services;
    }

}
