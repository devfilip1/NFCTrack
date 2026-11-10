using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShortLink.Application.Auth;
using ShortLink.Application.Links;
using ShortLink.Domain;

namespace ShortLink.Api.ErrorHandling;

internal sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int Status, string Title)? error = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Invalid request"),
            SlugAlreadyInUseException => (StatusCodes.Status409Conflict, "Slug is already in use"),
            RegistrationFailedException => (StatusCodes.Status400BadRequest, "Registration failed"),
            _ => null
        };

        if (error is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = error.Value.Status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = error.Value.Status,
                Title = error.Value.Title,
                Detail = exception.Message
            }
        });
    }
}
