using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ShortLink.Domain.Links;

public static partial class Slug
{
    public const int MinLength = 3;
    public const int MaxLength = 32;
    public const int GeneratedLength = 7;

    private const string Alphabet = "abcdefghjkmnpqrstuvwxyz23456789";

    private static readonly HashSet<string> Reserved = ["api", "openapi", "health"];

    public static string Generate() => RandomNumberGenerator.GetString(Alphabet, GeneratedLength);

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();

    public static bool IsValid(string slug) =>
        slug.Length is >= MinLength and <= MaxLength
        && Pattern().IsMatch(slug)
        && !Reserved.Contains(slug);

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex Pattern();
}
