using Microsoft.AspNetCore.Builder;

namespace NetCore_Learning.API.Middleware;

public static class UserSessionValidationExtensions
{
    public static IApplicationBuilder UseRequestHeaderValidation(this IApplicationBuilder app)
        => app.UseMiddleware<RequestHeaderValidationMiddleware>();
}


