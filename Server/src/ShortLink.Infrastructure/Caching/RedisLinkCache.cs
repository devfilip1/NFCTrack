using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using ShortLink.Application.Links;
using ShortLink.Infrastructure.Resilience;

namespace ShortLink.Infrastructure.Caching;

/// <summary>
/// Redis-backed link cache. Every failure falls back silently to the database (the caller treats
/// a miss as "go to SQL Server"), and the circuit breaker stops calling Redis while it is down so
/// redirects do not wait on connection timeouts.
/// </summary>
internal sealed class RedisLinkCache(
    IDistributedCache cache,
    ILogger<RedisLinkCache> logger,
    ResiliencePipelineProvider<string> pipelines) : ILinkCache
{
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(24);

    private readonly ResiliencePipeline _pipeline = pipelines.GetPipeline(ResiliencePipelines.RedisCache);

    private static string Key(string slug) => $"links:destination:{slug}";

    public async Task<string?> GetDestinationAsync(string slug, CancellationToken cancellationToken)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async token => await cache.GetStringAsync(Key(slug), token),
                cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open: Redis is known to be down and the failure was already logged.
            return null;
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
            await _pipeline.ExecuteAsync(
                async token => await cache.RemoveAsync(Key(slug), token),
                cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open: skip the cache, the entry expires on its own.
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
            await _pipeline.ExecuteAsync(
                async token => await cache.SetStringAsync(Key(slug), destinationUrl, options, token),
                cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open: skip caching, the next request reads from the database again.
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to write slug {Slug} to cache", slug);
        }
    }
}
