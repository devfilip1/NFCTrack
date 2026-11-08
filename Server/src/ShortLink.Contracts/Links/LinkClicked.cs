namespace ShortLink.Contracts.Links;

public sealed record LinkClicked(string Slug, DateTimeOffset OccurredAt, string? UserAgent, string? Referrer);
