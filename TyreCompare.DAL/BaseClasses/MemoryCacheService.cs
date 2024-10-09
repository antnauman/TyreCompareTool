using Microsoft.Extensions.Caching.Memory;

namespace TyreCompare.DAL.BaseClasses;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache MemoryCache;
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        MemoryCache = memoryCache;
    }

    public T Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        { return default(T); }

        MemoryCache.TryGetValue(key, out T value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        { return; }

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        if (expiration != null)
        { cacheEntryOptions.AbsoluteExpirationRelativeToNow = expiration; }

        MemoryCache.Set(key, value, cacheEntryOptions);
    }

    public void Delete<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        { return; }

        MemoryCache.Remove(key);
    }
}
