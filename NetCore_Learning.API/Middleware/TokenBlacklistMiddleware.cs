using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NetCore_Learning.Infrastructure.Services.Caching;
using NetCore_Learning.Share.Common;
using NetCore_Learning.Share.Helper;
using Newtonsoft.Json;

namespace NetCore_Learning.API.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;

    public TokenBlacklistMiddleware(
        RequestDelegate next,
        ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

        // Skip if endpoint has [AllowAnonymous]
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        // Extract token from Authorization header
        var token = ExtractTokenFromHeader(context);
        if (string.IsNullOrWhiteSpace(token))
        {
            // No token, let JWT middleware handle it
            await _next(context);
            return;
        }

        // Resolve scoped service from request scope (middleware is singleton, cannot inject scoped service)
        var redisCacheService = context.RequestServices.GetRequiredService<IRedisCacheService>();

        // Check if token is in blacklist
        if (await IsTokenBlacklistedAsync(token, redisCacheService))
        {
            _logger.LogWarning("Attempted to use blacklisted token");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var errorBody = new
            {
                success = false,
                message = "Token has been revoked",
                code = "TOKEN_BLACKLISTED"
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorBody));
            return;
        }

        await _next(context);
    }

    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }

    private async Task<bool> IsTokenBlacklistedAsync(string token, IRedisCacheService redisCacheService)
    {
        try
        {
            // Hash token to check in Redis
            var tokenHash = HashHelper.HashToken(token);
            var blacklistKey = $"{RedisCachingOptionsEnum.BlacklistToken}_{tokenHash}";

            // Check if token exists in blacklist
            var blacklistedData = await redisCacheService.GetDataAsync<byte[]>(blacklistKey);
            return blacklistedData != null;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error checking token blacklist");
            return false;
        }
    }
}

