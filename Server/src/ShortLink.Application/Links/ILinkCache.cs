using System;
using System.Collections.Generic;
using System.Text;

namespace ShortLink.Application.Links;

public interface ILinkCache
{
    public Task<string?> GetDestinationAsync(string slug, CancellationToken cancellationToken);

    public Task SetDestinationAsync(string slug, string destinationUrl, CancellationToken cancellationToken);

    public Task RemoveAsync(string slug, CancellationToken cancellationToken);
}
