using System.Net.Mime;
using System.Text.Json;
using HandGame.Api.Random;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HandGame.Api.Infra;

public static class ExceptionHandlingExtensions
{
    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                ProblemDetails details;
                IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionFeature != null)
                {
                    if (exceptionFeature.Error is GenerateFailureException)
                    {
                        details = ProcessGenerateFailureException(context);
                    }
                    else
                    {
                        details = ProcessUnexpectedError("Unexpected error", context);
                    }
                }
                else
                {
                    details = ProcessUnexpectedError("Unknown error", context);
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(details));
            });
        });
    }

    private static ProblemDetails ProcessGenerateFailureException(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        return new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "Error",
            Detail = "Try again soon"
        };
    }

    private static ProblemDetails ProcessUnexpectedError(string errorMessage, HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        return new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "Error",
            Detail = errorMessage
        };
    }
}
