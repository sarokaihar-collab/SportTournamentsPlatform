using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportTournaments.Domain.Exceptions;
using System.Text.Json;

namespace SportTournaments.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, type, message) = exception switch
        {
            DomainValidationException => (
                StatusCodes.Status400BadRequest,
                "Domain validation failed",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                "The submitted data violates a domain rule."),

            DomainOperationNotAllowedException => (
                StatusCodes.Status409Conflict,
                "Operation is not allowed",
                "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                "The operation is not allowed in the current state."),

            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                "The resource was modified by another request. Reload it and try again."),

            _ => (0, string.Empty, string.Empty, string.Empty)
        };

        if (statusCode == 0)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = message
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        if (await problemDetailsService.TryWriteAsync(context))
        {
            return true;
        }

        httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            problemDetails,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);

        return true;
    }
}