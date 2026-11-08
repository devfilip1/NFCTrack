using Microsoft.AspNetCore.Http.HttpResults;
using ShortLink.Application.Links;

namespace ShortLink.Api.Endpoints;

public static class RedirectEndpoints
{
    public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{slug}", RedirectAsync).WithName("RedirectToDestination").WithTags("Redirect");

        return endpoints;
    }


    private static async Task<Results<RedirectHttpResult, NotFound>> RedirectAsync(string slug, HttpRequest request, LinkService service,CancellationToken cancellationToken)
    {
        var link = await service.ResolveDestinationAsync(slug, request.Headers.UserAgent.ToString(), request.Headers.Referer.ToString(), cancellationToken);

        if (link is null)
            return TypedResults.NotFound();

        return TypedResults.Redirect(link);
    }
}
