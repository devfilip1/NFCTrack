using Microsoft.AspNetCore.Identity;
using ShortLink.Application.Auth;

namespace ShortLink.Infrastructure.Identity;

internal sealed class IdentityAuthService(UserManager<ApplicationUser> userManager, JwtTokenGenerator tokenGenerator) : IAuthService
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return null;

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            return null;

        return tokenGenerator.Generate(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser { Email = request.Email, UserName = request.Email };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new RegistrationFailedException(result.Errors.Select(e => e.Description).ToList());

        return tokenGenerator.Generate(user);
    }
}
