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
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

        // Bỏ qua nếu có [AllowAnonymous]
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        // Các header FE gửi lên
        var deviceId = context.Request.Headers["X-Device-Id"].ToString();
        var userAgent = context.Request.Headers["X-User-Agent"].ToString();
        var ipAddress = context.Request.Headers["X-IP-Address"].ToString();

        // Kiểm tra thiếu header nào
        var missingHeaders = new List<string>();
        if (string.IsNullOrWhiteSpace(deviceId)) missingHeaders.Add("X-Device-Id");
        if (string.IsNullOrWhiteSpace(userAgent)) missingHeaders.Add("X-User-Agent");
        if (string.IsNullOrWhiteSpace(ipAddress)) missingHeaders.Add("X-IP-Address");

        if (missingHeaders.Any())
        {
            var msg = $"Missing some informations in header: {string.Join(", ", missingHeaders)}";
            _logger.LogWarning(msg);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var errorBody = new InvalidDataResponseResult<string>(
                $"Missing headers: {string.Join(", ", missingHeaders)}"
            );

            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorBody));
            return;
        }

        // Lưu vào HttpContext.Items để dùng ở tầng dưới
        context.Items["DeviceId"] = deviceId;
        context.Items["UserAgent"] = userAgent;
        context.Items["IpAddress"] = ipAddress;

        await _next(context);
    }
}


