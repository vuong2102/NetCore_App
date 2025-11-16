namespace NetCore_Learning.Infrastructure.Services.Caching;

public interface IRedisCacheService
{
    Task<T?> GetDataAsync<T>(string key);
    Task SetDataAsync<T>(string key, T data, int minutes);
    Task RemoveDataAsync(string key);
    Task<List<string>> GetKeysByPatternAsync(string pattern);
    Task RemoveMultipleKeysAsync(List<string> keys);
}