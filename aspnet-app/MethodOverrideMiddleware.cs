// MethodOverrideMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class MethodOverrideMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MethodOverrideMiddleware> _logger;

    public MethodOverrideMiddleware(RequestDelegate next, ILogger<MethodOverrideMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var overrideHeaderNames = new[] {
            "X-HTTP-Method-Override",
            "X-Method-Override",
            "X-HTTP-Method"
        };

        string originalMethod = context.Request.Method;
        string? overrideMethod = null;

        foreach (var headerName in overrideHeaderNames)
        {
            if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
            {
                overrideMethod = headerValue.ToString().ToUpperInvariant();
                _logger.LogInformation("!!! Backend detected {HeaderName}: '{OverrideMethod}' (Original Method: {OriginalMethod}) !!!", headerName, overrideMethod, originalMethod);
                break;
            }
        }

        if (!string.IsNullOrEmpty(overrideMethod))
        {
            context.Request.Method = overrideMethod; // <-- ¡Aquí es donde sobrescribe el método!
            _logger.LogInformation("Método de solicitud SOBREESCRITO a: {ProcessedMethod}", context.Request.Method);
        }
        else
        {
            _logger.LogInformation("No se encontró encabezado de sobrescritura de método. Usando método original: {OriginalMethod}", originalMethod);
        }

        await _next(context);
    }
}

public static class MethodOverrideMiddlewareExtensions
{
    public static IApplicationBuilder UseMethodOverride(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MethodOverrideMiddleware>();
    }
}