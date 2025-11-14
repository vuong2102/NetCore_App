using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace NetCore_Learning.Infrastructure.Services.Caching;

public class RedisCacheService 
    (IDistributedCache distributedCache) : IRedisCacheService
{
    public async Task<T?> GetDataAsync<T>(string key)
    {
        var data = await distributedCache.GetStringAsync(key);
        if (data is null)
            return default;
        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetDataAsync<T>(string key, T data, int minutes)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes)
        };
        var jsonData = JsonSerializer.Serialize(data);
        await distributedCache.SetStringAsync(key, jsonData, options);
    }

    public async Task RemoveDataAsync(string key)
    {
        await distributedCache.RemoveAsync(key);
    }

}