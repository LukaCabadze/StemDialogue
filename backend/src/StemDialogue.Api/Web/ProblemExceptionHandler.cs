using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Common;

namespace StemDialogue.Api.Web;

/// <summary>
/// Maps exceptions to RFC 7807 problem responses. Internal details are logged, never returned.
/// </summary>
internal sealed class ProblemExceptionHandler(IProblemDetailsService problems, ILogger<ProblemExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is OperationCanceledException && http.RequestAborted.IsCancellationRequested)
        {
            http.Response.StatusCode = 499; // client closed request
            return true;
        }

        if (exception is SlugMovedException moved)
        {
            WriteRedirect(http, moved.NewSlug);
            return true;
        }

        ProblemDetails problem = exception switch
        {
            ValidationException validation => Validation(validation),
            NotFoundException notFound => Problem(StatusCodes.Status404NotFound, "Not found", notFound.Message),
            ConflictException conflict => Problem(StatusCodes.Status409Conflict, "Conflict", conflict.Message),
            DomainRuleException rule => Problem(StatusCodes.Status409Conflict, "Not allowed in the current state", rule.Message),
            AuthenticationFailedException auth => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", auth.Message),
            ForbiddenException forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message),
            BadHttpRequestException bad => Problem(bad.StatusCode, "Bad request", bad.StatusCode == StatusCodes.Status413PayloadTooLarge ? "The upload is too large." : "The request is invalid."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred. Please try again."),
        };

        if (problem.Status >= 500)
            logger.LogError(exception, "Unhandled failure for {Method} {Path}", http.Request.Method, http.Request.Path);
        else if (exception is ConflictException { InnerException: not null })
            logger.LogWarning(exception, "Conflict for {Method} {Path}", http.Request.Method, http.Request.Path);

        problem.Extensions["traceId"] = http.TraceIdentifier;
        http.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return await problems.TryWriteAsync(new ProblemDetailsContext { HttpContext = http, ProblemDetails = problem, Exception = exception });
    }

    private static ProblemDetails Problem(int status, string title, string detail) => new() { Status = status, Title = title, Detail = detail };

    private static ValidationProblemDetails Validation(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName ?? string.Empty))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more fields are invalid.",
        };
    }

    /// <summary>A renamed slug: 301 to the same endpoint with the canonical slug (old links keep working).</summary>
    private static void WriteRedirect(HttpContext http, string newSlug)
    {
        var path = http.Request.Path.Value ?? string.Empty;
        var location = path[..(path.LastIndexOf('/') + 1)] + Uri.EscapeDataString(newSlug) + http.Request.QueryString;
        http.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        http.Response.Headers.Location = location;
    }
}
