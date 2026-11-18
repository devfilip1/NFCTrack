using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using ShortLink.Api.Auth;
using ShortLink.Api.Endpoints;
using ShortLink.Api.ErrorHandling;
using ShortLink.Api.RateLimiting;
using ShortLink.Application.Clicks;
using ShortLink.Application.Links;
using ShortLink.Infrastructure;
using ShortLink.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInfrastructureHealthChecks(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiRateLimiting();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<ClickService>();

// Read the real client IP from the X-Forwarded-For header sent by nginx, so rate limits are per
// user and not per proxy. Clearing the lists trusts the header from ANY sender, which is only
// acceptable in development: in production, list just the reverse proxy in KnownProxies.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    await app.Services.ApplyMigrationsAsync();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapLinkEndpoints();
app.MapRedirectEndpoints();

app.Run();
