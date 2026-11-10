namespace ShortLink.Application.Auth;

public sealed class RegistrationFailedException(IReadOnlyList<string> errors) : Exception(string.Join(" ", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
