using Microsoft.Extensions.Primitives;

namespace CreditApi.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    // Rutas que NO queremos proteger (swagger, health, archivos estáticos, etc)
    private static readonly string[] _whitelistPaths = new[]
    {
        "/swagger", "/swagger/index.html", "/swagger/v1/swagger.json"
    };

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _apiKey = config["Security:ApiKey"] ?? "";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Permitir swagger y GET a raíz si lo usas
        if (_whitelistPaths.Any(w => path.StartsWith(w, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Solo proteger la API (rutas /api/*). Si quieres proteger todo, quita esta condición.
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-api-key", out StringValues key) || key != _apiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key inválida o ausente.");
            return;
        }

        await _next(context);
    }
}
