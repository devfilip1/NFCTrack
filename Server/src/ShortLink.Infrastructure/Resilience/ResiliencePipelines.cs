namespace ShortLink.Infrastructure.Resilience;

internal static class ResiliencePipelines
{
    public const string RedisCache = "redis-cache";
    public const string ClickPublish = "click-publish";
}
