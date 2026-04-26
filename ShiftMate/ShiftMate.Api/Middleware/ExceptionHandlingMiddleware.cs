using FluentValidation;
using ShiftMate.Application.Common.Exceptions;
using System.Text.Json;

namespace ShiftMate.Api.Middleware;

// Global felhantering. Mappar kastade exceptions från handlers till lämpliga HTTP-statuskoder
// så att controllers kan skippa try/catch och bara delegera till MediatR.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, payload) = Map(ex);

        if (statusCode >= 500)
        {
            _logger.LogError(ex, "Ohanterat fel vid {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (int StatusCode, object Payload) Map(Exception ex) => ex switch
    {
        ValidationException vex => (
            StatusCodes.Status400BadRequest,
            new
            {
                Error = true,
                Message = "Valideringsfel: " + vex.Message,
                Details = vex.Errors.Select(e => e.ErrorMessage)
            }),

        EmailNotVerifiedException enve => (
            StatusCodes.Status400BadRequest,
            new { Error = true, Message = enve.Message, Code = "EMAIL_NOT_VERIFIED" }),

        NotFoundException nfe => (
            StatusCodes.Status404NotFound,
            new { Error = true, Message = nfe.Message }),

        ForbiddenException fe => (
            StatusCodes.Status403Forbidden,
            new { Error = true, Message = fe.Message }),

        ConflictException ce => (
            StatusCodes.Status409Conflict,
            new { Error = true, Message = ce.Message }),

        InvalidOperationException ioe => (
            StatusCodes.Status400BadRequest,
            new { Error = true, Message = ioe.Message }),

        ArgumentException ae => (
            StatusCodes.Status400BadRequest,
            new { Error = true, Message = ae.Message }),

        UnauthorizedAccessException uae => (
            StatusCodes.Status401Unauthorized,
            new { Error = true, Message = uae.Message }),

        // Generella business-fel som handlers kastar med "throw new Exception" → 400
        // (för bakåtkompatibilitet tills handlers migreras till specifika exception-typer)
        _ when ex.GetType() == typeof(Exception) => (
            StatusCodes.Status400BadRequest,
            new { Error = true, Message = ex.Message }),

        // Allt annat oväntat → 500. Dölj detaljer i prod, visa i dev.
        _ => (
            StatusCodes.Status500InternalServerError,
            new
            {
                Error = true,
                Message = _env.IsDevelopment()
                    ? $"Ett internt fel uppstod: {ex.Message}"
                    : "Ett internt fel uppstod."
            })
    };
}