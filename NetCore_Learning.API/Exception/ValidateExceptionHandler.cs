using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace NetCore_Learning.API.Exception
{
    internal sealed class ValidateExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidateExceptionHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProblemDetailsService _problemDetailsService;

        public ValidateExceptionHandler(ILogger<ValidateExceptionHandler> logger, IConfiguration configuration, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _configuration = configuration;
            _problemDetailsService = problemDetailsService; //get the details of the problem
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
        {
            var isDevelopmentEnviroment = _configuration.GetValue<bool>("Environment:Development");
            if (exception is not ValidationException validationException)
            {
                return false;
            } 
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var context = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = isDevelopmentEnviroment ? validationException.Message : "Some values are required",
                    Detail = isDevelopmentEnviroment ? validationException.InnerException?.Message : null,
                    Type = validationException.GetType().Name
                }
            };
            var errors = validationException.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.ProblemDetails.Extensions["errors"] = errors;
            return await _problemDetailsService.TryWriteAsync(context);
        }
    }
}
