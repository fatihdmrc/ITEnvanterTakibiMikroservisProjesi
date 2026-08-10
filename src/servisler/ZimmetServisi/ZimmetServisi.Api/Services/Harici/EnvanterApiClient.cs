namespace ZimmetServisi.Api.Services.Harici;

public sealed class EnvanterApiClient(HttpClient httpClient) : HariciApiClientBase(httpClient)
{
    public Task<HariciApiSonucu<HariciCihazCevap>> CihazGetirAsync(Guid cihazId, string bearerToken, CancellationToken cancellationToken)
        => GetAsync<HariciCihazCevap>($"/api/cihazlar/{cihazId}", bearerToken, cancellationToken);

    public Task<HariciApiSonucu<HariciCihazCevap>> CihazDurumHareketiIsleAsync(
        Guid cihazId,
        HariciCihazDurumHareketiIstek istek,
        string bearerToken,
        CancellationToken cancellationToken)
        => PostAsync<HariciCihazCevap>($"/api/cihazlar/{cihazId}/durum-hareketleri", istek, bearerToken, cancellationToken);
}
