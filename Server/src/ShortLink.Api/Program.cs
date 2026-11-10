using Scalar.AspNetCore;
using ShortLink.Api.Endpoints;
using ShortLink.Api.ErrorHandling;
using ShortLink.Application.Clicks;
using ShortLink.Application.Links;
using ShortLink.Infrastructure;
using ShortLink.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<ClickService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    await app.Services.ApplyMigrationsAsync();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapLinkEndpoints();
app.MapRedirectEndpoints();

app.Run();
