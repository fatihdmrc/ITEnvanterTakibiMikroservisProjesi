namespace EnvanterServisi.Api.Services.Cache;

public interface IReferansVeriCacheServisi
{
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> veriUret,
        TimeSpan gecerlilikSuresi,
        CancellationToken cancellationToken = default);

    Task SilAsync(string key, CancellationToken cancellationToken = default);
}
