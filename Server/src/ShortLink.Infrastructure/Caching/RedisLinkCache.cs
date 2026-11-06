using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShortLink.Application.Links;

namespace ShortLink.Infrastructure.Caching;

internal sealed class RedisLinkCache(IDistributedCache cache, ILogger<RedisLinkCache> logger) : ILinkCache
{
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(24);

    private static string Key(string slug) => $"links:destination:{slug}";

    public async Task<string?> GetDestinationAsync(string slug, CancellationToken cancellationToken)
    {
        try 
        { 
            return await cache.GetStringAsync(Key(slug), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to read slug {Slug} from cache", slug);
            return null;
        }
    }

    public async Task RemoveAsync(string slug, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(Key(slug), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to remove slug {Slug} from cache", slug);
        }
    }

    public async Task SetDestinationAsync(string slug, string destinationUrl, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Expiration
        };

        try
        {
            await cache.SetStringAsync(Key(slug), destinationUrl, options, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to write slug {Slug} to cache", slug);
        }
    }
}
