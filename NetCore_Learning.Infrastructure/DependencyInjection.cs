using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace NetCore_Learning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis (IDistributedCache) registration
        var redisConnection = configuration["Redis:ConnectionString"];
        var redisInstance = configuration["Redis:InstanceName"];

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            // Register IDistributedCache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                if (!string.IsNullOrWhiteSpace(redisInstance))
                {
                    options.InstanceName = redisInstance;
                }
            });

            // Register IConnectionMultiplexer để hỗ trợ pattern search
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(redisConnection);
            });
        }

        return services;
    }
}


