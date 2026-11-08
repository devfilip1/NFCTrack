using MassTransit;
using Microsoft.Extensions.Logging;
using ShortLink.Application.Clicks;
using ShortLink.Contracts.Links;

namespace ShortLink.Infrastructure.Messaging;

internal sealed class MassTransitClickTracker(IPublishEndpoint publishEndpoint, ILogger<MassTransitClickTracker> logger) : IClickTracker
{
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(2);

    public async Task TrackAsync(string slug, DateTimeOffset occurredAt, string? userAgent, string? referrer, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeout.CancelAfter(PublishTimeout);

        try
        {
            await publishEndpoint.Publish(new LinkClicked(slug, occurredAt, userAgent, referrer), timeout.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Failed to publish click for slug {Slug}", slug);
        }
    }
}
