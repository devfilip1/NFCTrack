namespace ShortLink.Api.RateLimiting;

public static class RateLimitPolicies
{
    /// <summary>Login and registration: slows down password brute force.</summary>
    public const string Auth = "auth";

    /// <summary>Public redirects: generous, since a whole venue can share one public IP.</summary>
    public const string Redirect = "redirect";
}
