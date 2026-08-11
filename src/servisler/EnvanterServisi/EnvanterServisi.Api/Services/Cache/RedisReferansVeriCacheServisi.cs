using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace EnvanterServisi.Api.Services.Cache;

public sealed class RedisReferansVeriCacheServisi(
    IDistributedCache distributedCache,
    ILogger<RedisReferansVeriCacheServisi> logger) : IReferansVeriCacheServisi
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> veriUret,
        TimeSpan gecerlilikSuresi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheDegeri = await distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cacheDegeri))
            {
                var veri = JsonSerializer.Deserialize<T>(cacheDegeri, JsonAyarlari);
                if (veri is not null)
                {
                    return veri;
                }
            }
        }
        catch (Exception exception) when (CacheHatasiMi(exception))
        {
            logger.LogWarning(exception, "Redis cache okunamadi. Key: {CacheKey}", key);
        }

        var guncelVeri = await veriUret(cancellationToken);

        try
        {
            var json = JsonSerializer.Serialize(guncelVeri, JsonAyarlari);
            await distributedCache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = gecerlilikSuresi
                },
                cancellationToken);
        }
        catch (Exception exception) when (CacheHatasiMi(exception))
        {
            logger.LogWarning(exception, "Redis cache yazilamadi. Key: {CacheKey}", key);
        }

        return guncelVeri;
    }

    public async Task SilAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception exception) when (CacheHatasiMi(exception))
        {
            logger.LogWarning(exception, "Redis cache temizlenemedi. Key: {CacheKey}", key);
        }
    }

    private static bool CacheHatasiMi(Exception exception)
        => exception is TimeoutException
            or InvalidOperationException
            or JsonException
            or OperationCanceledException
            or StackExchange.Redis.RedisException;
}
