using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace WebAPI.Infrastructure.ExceptionHandling;

public class DefaultExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public DefaultExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException vex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = vex.Errors
               .ToLookup(failure => failure.PropertyName)
               .ToDictionary(
                failures => failures.Key,
                failures => failures.Select(failure => failure.ErrorMessage));

            return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = "An validation error occurred",
                    Detail ="validation-error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Type = "validation-error",
                    Extensions = new Dictionary<string, object?>(){ { "errors", errors } }
                },
                Exception = exception
            });
        }

        if (exception is NotFoundException nfex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

            return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = "An not found error occurred",
                    Detail = nfex.Message,
                    Status = (int)HttpStatusCode.NotFound,
                    Type = "resource-not-found",
                },
                Exception = exception
            });
        }

        if (exception is InfrastructureException iex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

            return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = "An infrastructure error occurred",
                    Detail = iex.Message,
                    Status = (int)HttpStatusCode.NotFound,
                    Type = "infrastructure-error",
                },
                Exception = exception
            });
        }

        if (exception is DomainException dex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = "An domian error occurred",
                    Detail = dex.Message,
                    Status = (int)HttpStatusCode.BadRequest,
                    Type = "domain-error",
                },
                Exception = exception
            });
        }

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
                {
                    Type = "internal-server-error",
                    Detail = exception.StackTrace,
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = exception.Message
                },
            Exception = exception
        });
    }
}