using MassTransit;
using ShortLink.Application.Clicks;
using ShortLink.Contracts.Links;

namespace ShortLink.Worker.Consumers;

public sealed class LinkClickedConsumer(ClickService clickService, ILogger<LinkClickedConsumer> logger) : IConsumer<LinkClicked>
{
    public async Task Consume(ConsumeContext<LinkClicked> context)
    {
        var message = context.Message;

        var recorded = await clickService.RecordAsync(message.Slug, message.OccurredAt, message.UserAgent, message.Referrer, context.CancellationToken);

        if (!recorded)
            logger.LogInformation("Ignored click for unknown slug {Slug}", message.Slug);
    }
}
