namespace ZimmetServisi.Api.Services.Harici;

public sealed class KimlikPersonelApiClient(HttpClient httpClient) : HariciApiClientBase(httpClient)
{
    public Task<HariciApiSonucu<HariciPersonelCevap>> PersonelGetirAsync(Guid personelId, string bearerToken, CancellationToken cancellationToken)
        => GetAsync<HariciPersonelCevap>($"/api/personeller/{personelId}", bearerToken, cancellationToken);
}
