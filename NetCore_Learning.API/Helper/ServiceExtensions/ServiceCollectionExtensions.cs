using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetCore_Learning.API.Exception;
using NetCore_Learning.Application.Mappings;
using NetCore_Learning.Application.Services.Implement;
using NetCore_Learning.Application.Services.Interface;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Core;
using NetCore_Learning.Data.Core.YourApp.Core.Interfaces;
using NetCore_Learning.Data.Helper;
using NetCore_Learning.Infrastructure;
using NetCore_Learning.Infrastructure.Services.Caching;

namespace NetCore_Learning.API.Helper.ServiceExtensions;

/// <summary>
/// Extension class to register all services into DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all services for the application
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Exception Handlers
        services.AddExceptionHandlers();

        // 2. Database Context
        services.AddDatabaseContext(configuration);

        // 3. HttpContext Accessor
        services.AddHttpContextAccessor();

        // 4. Authentication & Authorization
        services.AddJwtAuthentication(configuration);

        // 5. Infrastructure Services
        services.AddInfrastructureServices(configuration);

        // 6. Application Services
        services.AddApplicationServicesInternal();

        // 7. API Services
        services.AddApiServices();

        // 8. Mapping Services
        services.AddMappingServices();

        return services;
    }

    /// <summary>
    /// Register Exception Handlers
    /// </summary>
    private static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddProblemDetails(configure =>
        {
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddExceptionHandler<ValidateExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Register Database Context
    /// </summary>
    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    /// <summary>
    /// Register JWT Authentication
    /// </summary>
    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:ValidIssuer"],
                    ValidAudience = configuration["Jwt:ValidAudience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? string.Empty))
                };
            });

        return services;
    }

    /// <summary>
    /// Register Infrastructure Services (Repository, Redis, etc.)
    /// </summary>
    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRepository();
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Register Application Services (Business Logic Services)
    /// Automatically register all services in Services/Interface and Services/Implement folders
    /// </summary>
    private static IServiceCollection AddApplicationServicesInternal(this IServiceCollection services)
    {
        // Unit of Work (not automatically registered because it doesn't follow the convention)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Redis Cache Service (not automatically registered because it belongs to Infrastructure)
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        // Automatically register all services according to the folder structure
        services.RegisterServicesFromAssembly();

        return services;
    }

    /// <summary>
    /// Register API Services (Controllers, Versioning, OpenAPI, etc.)
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddApiVersioningService();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        return services;
    }

    /// <summary>
    /// Register Mapping Services (Mapster)
    /// </summary>
    private static IServiceCollection AddMappingServices(this IServiceCollection services)
    {
        // Register Mapster mappings
        RegisterMapsterConfig.RegisterMappings();
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    /// <summary>
    /// Automatically register all services from assembly
    /// Find all interfaces in Services.Interface namespace
    /// and corresponding implementations in Services.Implement namespace
    /// </summary>
    private static IServiceCollection RegisterServicesFromAssembly(this IServiceCollection services)
    {
        var interfaceNamespace = "NetCore_Learning.Application.Services.Interface";
        var implementationNamespace = "NetCore_Learning.Application.Services.Implement";
        
        // Get assembly containing services
        var assembly = Assembly.GetAssembly(typeof(IAccountService)) 
            ?? throw new InvalidOperationException("Cannot find Application assembly");

        // Get all interfaces in Services.Interface namespace
        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface 
                       && t.Namespace == interfaceNamespace 
                       && t.Name.StartsWith("I")
                       && t.Name.EndsWith("Service"))
            .ToList();

        foreach (var interfaceType in interfaces)
        {
            // Find the corresponding implementation
            // Example: IAccountService -> AccountService
            var implementationName = interfaceType.Name.Substring(1); // Remove the "I" prefix
            
            var implementationType = assembly.GetTypes()
                .FirstOrDefault(t => t.Namespace == implementationNamespace
                                    && t.Name == implementationName
                                    && !t.IsInterface
                                    && !t.IsAbstract
                                    && interfaceType.IsAssignableFrom(t));

            if (implementationType != null)
            {
                // Register service with Scoped lifetime
                services.AddScoped(interfaceType, implementationType);
            }
            else
            {
                // Log warning if implementation is not found
                // Can throw exception or log warning as needed
                System.Diagnostics.Debug.WriteLine(
                    $"Warning: Cannot find implementation for interface {interfaceType.Name}");
            }
        }

        return services;
    }
}

