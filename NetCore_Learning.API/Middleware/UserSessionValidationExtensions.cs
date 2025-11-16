using Microsoft.AspNetCore.Builder;

namespace NetCore_Learning.API.Middleware;

/// <summary>
/// Extension methods to register custom middleware
/// 
/// HOW TO ADD NEW MIDDLEWARE:
/// 1. Create middleware class in Middleware folder (e.g., MyNewMiddleware.cs)
/// 2. Create extension method below (e.g., UseMyNewMiddleware)
/// 3. Add to UseCustomMiddlewares() in the correct order
/// 
/// NOTE: Middleware order is very important, think carefully before adding
/// </summary>
public static class UserSessionValidationExtensions
{
    /// <summary>
    /// Register all custom middleware in the correct order
    /// Middleware order is very important, should not be changed arbitrarily
    /// </summary>
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // Middleware order (from top to bottom):
        // 1. ScalarTestHeaders - Add default headers for Scalar testing (Development only)
        // 2. TokenBlacklist - Check token blacklist (after Authentication, before Authorization)
        // 3. RequestHeaderValidation - Validate request headers
        
        app.UseScalarTestHeaders();
        app.UseTokenBlacklist();
        app.UseRequestHeaderValidation();
        
        // Add new middleware here in the correct order
        // app.UseMyNewMiddleware();
        
        return app;
    }

    /// <summary>
    /// Register middleware to add default headers for Scalar testing
    /// </summary>
    public static IApplicationBuilder UseScalarTestHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<ScalarTestHeadersMiddleware>();

    /// <summary>
    /// Register middleware to check token blacklist
    /// </summary>
    public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder app)
        => app.UseMiddleware<TokenBlacklistMiddleware>();

    /// <summary>
    /// Register middleware to validate request headers
    /// </summary>
    public static IApplicationBuilder UseRequestHeaderValidation(this IApplicationBuilder app)
        => app.UseMiddleware<RequestHeaderValidationMiddleware>();


}


