using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SportTournaments.Api.ExceptionHandling;
using SportTournaments.Domain.Exceptions;
using System.Text.Json;

namespace SportTournaments.UnitTests.Api.ExceptionHandling;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenDomainValidationException_Returns400()
    {
        var exception = new DomainValidationException("Tournament name cannot be empty.", "name");

        var (context, handled) = await ExecuteAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(400, problemDetails.GetProperty("status").GetInt32());
        Assert.Equal("Domain validation failed", problemDetails.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenDomainOperationIsNotAllowed_Returns409()
    {
        var exception = new DomainOperationNotAllowedException("Tournament cannot be cancelled in its current state.");

        var (context, handled) = await ExecuteAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(context);

        Assert.Equal(409, problemDetails.GetProperty("status").GetInt32());
        Assert.Equal("Operation is not allowed", problemDetails.GetProperty("title").GetString());
        Assert.Equal("The operation is not allowed in the current state.", problemDetails.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenConcurrencyException_Returns409WithoutExposingInternalDetails()
    {
        const string internalMessage = "Sensitive database details: internal table and query information.";

        var exception = new DbUpdateConcurrencyException(internalMessage);

        var (context, handled) = await ExecuteAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync(context);
        var responseJson = problemDetails.GetRawText();

        Assert.Equal("Concurrency conflict", problemDetails.GetProperty("title").GetString());
        Assert.Equal("The resource was modified by another request. Reload it and try again.", problemDetails.GetProperty("detail").GetString());
        Assert.DoesNotContain(internalMessage, responseJson);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsUnknown_ReturnsFalse()
    {
        var exception = new InvalidOperationException("Unexpected internal failure.");

        var (context, handled) = await ExecuteAsync(exception);

        Assert.False(handled);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    private static async Task<(DefaultHttpContext Context, bool Handled)>
        ExecuteAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handler = new ApiExceptionHandler(new NonWritingProblemDetailsService());

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        return (context, handled);
    }

    private static async Task<JsonElement> ReadProblemDetailsAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;

        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        return document.RootElement.Clone();
    }

    private sealed class NonWritingProblemDetailsService: IProblemDetailsService
    {
        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            return ValueTask.FromResult(false);
        }
    }
}