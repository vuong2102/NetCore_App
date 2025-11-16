using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace NetCore_Learning.Infrastructure.Services.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _instanceName;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _instanceName = configuration["Redis:InstanceName"] ?? string.Empty;
    }

    public async Task<T?> GetDataAsync<T>(string key)
    {
        // Nếu T là byte[], lấy raw bytes
        if (typeof(T) == typeof(byte[]))
        {
            var bytes = await _distributedCache.GetAsync(key);
            if (bytes == null)
                return default;
            return (T)(object)bytes;
        }

        // Nếu không, deserialize từ JSON string
        var data = await _distributedCache.GetStringAsync(key);
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
        await _distributedCache.SetStringAsync(key, jsonData, options);
    }

    public async Task RemoveDataAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }

    public async Task<List<string>> GetKeysByPatternAsync(string pattern)
    {
        var keys = new List<string>();
        var endpoints = _redis.GetEndPoints();
        if (endpoints.Length == 0)
            return keys;

        var server = _redis.GetServer(endpoints[0]);

        // Thêm instance name vào pattern để đồng bộ với IDistributedCache
        var searchPattern = string.IsNullOrWhiteSpace(_instanceName)
            ? pattern
            : $"{_instanceName}{pattern}";

        await foreach (var key in server.KeysAsync(pattern: searchPattern))
        {
            var keyString = key.ToString();
            
            // Loại bỏ instance name khi trả về cho user
            if (!string.IsNullOrWhiteSpace(_instanceName) && keyString.StartsWith(_instanceName))
            {
                keyString = keyString.Substring(_instanceName.Length);
            }
            
            keys.Add(keyString);
        }

        return keys;
    }

    public async Task RemoveMultipleKeysAsync(List<string> keys)
    {
        if (keys == null || keys.Count == 0)
            return;

        var db = _redis.GetDatabase();
        var redisKeys = keys.Select(key =>
        {
            // Thêm instance name vào key để đồng bộ với IDistributedCache
            var fullKey = string.IsNullOrWhiteSpace(_instanceName)
                ? key
                : $"{_instanceName}{key}";
            return (RedisKey)fullKey;
        }).ToArray();

        await db.KeyDeleteAsync(redisKeys);
    }
}