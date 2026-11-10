namespace ShortLink.Application.Auth;

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt);
