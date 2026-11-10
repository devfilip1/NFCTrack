using Microsoft.AspNetCore.Http.HttpResults;
using ShortLink.Application.Clicks;
using ShortLink.Application.Links;

namespace ShortLink.Api.Endpoints;

public static class LinkEndpoints
{
    public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/links").WithTags("Links");

        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetLinkById");
        group.MapPost("/", CreateAsync).WithName("CreateLink");
        group.MapGet("/", ListAsync).WithName("ListLinks");
        group.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateLink");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteLink");
        group.MapGet("/{id:guid}/stats", GetStatsAsync).WithName("GetLinkStats");

        return endpoints;
    }

    private static async Task<Results<Ok<LinkResponse>, NotFound>> GetByIdAsync(
        Guid id,
        LinkService service,
        CancellationToken cancellationToken)
    {
        var link = await service.GetByIdAsync(id, cancellationToken);

        if (link is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(link);
    }

    private static async Task<Created<LinkResponse>> CreateAsync(
        CreateLinkRequest request,
        LinkService service,
        CancellationToken cancellationToken)
    {
        var link = await service.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/links/{link.Id}", link);
    }

    private static async Task<Ok<IReadOnlyList<LinkResponse>>> ListAsync(
        LinkService service,
        CancellationToken cancellationToken)
    {
        var links = await service.ListAsync(cancellationToken);

        return TypedResults.Ok(links);
    }

    private static async Task<Results<Ok<LinkResponse>, NotFound>> UpdateAsync(Guid id,
        UpdateLinkRequest request,
        LinkService service,
        CancellationToken cancellationToken)
    {
        var link = await service.UpdateDestinationAsync(id, request, cancellationToken);

        if (link is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(link);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(Guid id,
        LinkService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<LinkStatsResponse>, NotFound>> GetStatsAsync(Guid id, int? days,
        ClickService service,
        CancellationToken cancellationToken)
    {
        var stats = await service.GetStatsAsync(id, days ?? ClickService.DefaultStatsDays, cancellationToken);

        if (stats is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(stats);
    }
}
