using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Net_Learning.Models.Models;
using NetCore_Learning.Infrastructure.Services.Caching;
using NetCore_Learning.Share.Common;
using Newtonsoft.Json;

namespace NetCore_Learning.API.Middleware;

public class RequestHeaderValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestHeaderValidationMiddleware> _logger;

    public RequestHeaderValidationMiddleware(RequestDelegate next, ILogger<RequestHeaderValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Step 1: Skip validation for paths that don't need headers (Swagger, OpenAPI, health checks, etc.)
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (ShouldSkipValidation(path))
        {
            await _next(context);
            return;
        }

        // Step 2: Get the routed endpoint
        var endpoint = context.GetEndpoint();
        
        // Step 3: Skip validation only if endpoint is not resolved (request doesn't match any route → will return 404)
        // Note: Even endpoints with [AllowAnonymous] need headers (e.g., login/register need DeviceId to save session)
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Step 4: Validate headers for all endpoints (including [AllowAnonymous] endpoints like login/register)
        // Headers are needed to save session information (DeviceId, UserAgent, IpAddress)
        var deviceId = context.Request.Headers["X-Device-Id"].ToString();
        var userAgent = context.Request.Headers["X-User-Agent"].ToString();
        var ipAddress = context.Request.Headers["X-IP-Address"].ToString();

        var missingHeaders = new List<string>();
        if (string.IsNullOrWhiteSpace(deviceId)) missingHeaders.Add("X-Device-Id");
        if (string.IsNullOrWhiteSpace(userAgent)) missingHeaders.Add("X-User-Agent");
        if (string.IsNullOrWhiteSpace(ipAddress)) missingHeaders.Add("X-IP-Address");

        if (missingHeaders.Any())
        {
            var msg = $"Missing required headers: {string.Join(", ", missingHeaders)}";
            _logger.LogWarning(msg);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var errorBody = new InvalidDataResponseResult<string>(
                $"Missing headers: {string.Join(", ", missingHeaders)}"
            );

            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorBody));
            return;
        }

        // Step 5: Store headers in HttpContext.Items for use in service layer
        context.Items["DeviceId"] = deviceId;
        context.Items["UserAgent"] = userAgent;
        context.Items["IpAddress"] = ipAddress;

        await _next(context);
    }

    /// <summary>
    /// Check if the path should skip validation
    /// </summary>
    private static bool ShouldSkipValidation(string path)
    {
        // Skip paths that don't need headers
        var skipPaths = new[]
        {
            "/swagger",
            "/openapi",
            "/scalar",
            "/health",
            "/favicon.ico",
            "/_vs",
            "/.well-known"
        };

        return skipPaths.Any(skipPath => path.StartsWith(skipPath, StringComparison.OrdinalIgnoreCase));
    }
}


