using Microsoft.AspNetCore.Http.HttpResults;
using ShortLink.Application.Auth;

namespace ShortLink.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync).WithName("Register");
        group.MapPost("/login", LoginAsync).WithName("Login");

        return endpoints;
    }

    private static async Task<Ok<AuthResponse>> RegisterAsync(RegisterRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> LoginAsync(LoginRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);

        if (response is null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(response);
    }
}
