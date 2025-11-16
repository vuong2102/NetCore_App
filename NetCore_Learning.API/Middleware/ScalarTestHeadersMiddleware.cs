using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NetCore_Learning.API.Middleware;

/// <summary>
/// Middleware to automatically add test headers when testing from Scalar
/// Only works in Development environment
/// </summary>
public class ScalarTestHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public ScalarTestHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only add headers in Development environment
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (environment.IsDevelopment())
        {
            // Add default headers if they don't exist (for testing)
            // This helps when testing from Scalar or any other tool
            if (!context.Request.Headers.ContainsKey("X-Device-Id"))
            {
                // Generate a unique device ID for testing
                // You can change this to a fixed value if you want consistent device ID during testing
                context.Request.Headers["X-Device-Id"] = "Iphone 99 Promax";
            }

            if (!context.Request.Headers.ContainsKey("X-User-Agent"))
            {
                // Use default user agent for testing
                context.Request.Headers["X-User-Agent"] = "Test-API-Client/1.0";
            }

            if (!context.Request.Headers.ContainsKey("X-IP-Address"))
            {
                // Try to get real IP, fallback to localhost
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                context.Request.Headers["X-IP-Address"] = ipAddress;
            }
        }

        await _next(context);
    }
}

