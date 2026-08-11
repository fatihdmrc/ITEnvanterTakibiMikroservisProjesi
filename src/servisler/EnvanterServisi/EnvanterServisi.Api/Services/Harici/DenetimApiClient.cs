using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterServisi.Api.Contracts.Denetim;

namespace EnvanterServisi.Api.Services.Harici;

public sealed class DenetimApiClient(HttpClient httpClient, ILogger<DenetimApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task CrudKaydiGonderAsync(CrudDenetimKaydiOlusturIstek istek, string? bearerToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(istek, JsonAyarlari);
            using var httpIstek = new HttpRequestMessage(HttpMethod.Post, "/api/denetim-kayitlari/crud")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                httpIstek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            using var cevap = await httpClient.SendAsync(httpIstek, cancellationToken);
            if (!cevap.IsSuccessStatusCode)
            {
                logger.LogWarning("Denetim kaydi gonderilemedi. DurumKodu: {DurumKodu}", cevap.StatusCode);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(exception, "Denetim kaydi servisine ulasilamadi. Ana islem devam etti.");
        }
    }
}
