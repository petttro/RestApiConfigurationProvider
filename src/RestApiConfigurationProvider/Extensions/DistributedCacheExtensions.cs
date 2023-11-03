using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace RestApiConfigurationProvider.Extensions;

internal static class DistributedCacheExtensions
{
    public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, TimeSpan cacheDuration, CancellationToken token = default)
    {
        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheDuration };
        await distributedCache.SetStringAsync(key, value.SerializeJsonSafe(), cacheOptions, token);
    }

    public static async Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        var result = await distributedCache.GetStringAsync(key, token);
        return result.DeserializeJsonSafe<T>();
    }
}
