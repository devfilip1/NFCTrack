using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using ShortLink.Application.Clicks;
using ShortLink.Contracts.Links;
using ShortLink.Infrastructure.Resilience;

namespace ShortLink.Infrastructure.Messaging;

/// <summary>
/// Publishes clicks without ever blocking a redirect for long: the pipeline retries transient
/// failures, caps the total wait at 2 seconds and stops trying while RabbitMQ is down.
/// A click that cannot be published is dropped (logged), the redirect still succeeds.
/// </summary>
internal sealed class MassTransitClickTracker(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitClickTracker> logger,
    ResiliencePipelineProvider<string> pipelines) : IClickTracker
{
    private readonly ResiliencePipeline _pipeline = pipelines.GetPipeline(ResiliencePipelines.ClickPublish);

    public async Task TrackAsync(string slug, DateTimeOffset occurredAt, string? userAgent, string? referrer, CancellationToken cancellationToken)
    {
        var message = new LinkClicked(slug, occurredAt, userAgent, referrer);

        try
        {
            await _pipeline.ExecuteAsync(
                async token => await publishEndpoint.Publish(message, token),
                cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            // Circuit is open: RabbitMQ is known to be down and the failure was already logged.
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Failed to publish click for slug {Slug}", slug);
        }
    }
}
