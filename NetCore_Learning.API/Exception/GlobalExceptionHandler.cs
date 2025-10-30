using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace NetCore_Learning.API.Exception
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _configuration = configuration;
            _problemDetailsService = problemDetailsService; //get the details of the problem
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);
            var statusCode = exception switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var isDevelopmentEnviroment = _configuration.GetValue<bool>("Environment:Development");

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = isDevelopmentEnviroment ? exception.Message : "An unexpected error occurred.",
                    Detail = isDevelopmentEnviroment ? exception.StackTrace : null,
                    Type = exception.GetType().Name
                }
            });
        }
    }
}
