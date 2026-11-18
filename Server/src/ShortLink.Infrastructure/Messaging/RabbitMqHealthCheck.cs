using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShortLink.Infrastructure.Messaging;

/// <summary>
/// Checks that the RabbitMQ broker accepts TCP connections. MassTransit's own bus check does not
/// notice an outage on a publish-only bus (the API has no consumers), so this covers that case.
/// </summary>
internal sealed class RabbitMqHealthCheck(string host, int port) : IHealthCheck
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();

            // WaitAsync stops waiting after the timeout even when the DNS lookup for a stopped
            // container ignores cancellation (it took ~8s with a token alone).
            await client.ConnectAsync(host, port).WaitAsync(Timeout, cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex) when (ex is SocketException or TimeoutException)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "RabbitMQ is unreachable.");
        }
    }
}
