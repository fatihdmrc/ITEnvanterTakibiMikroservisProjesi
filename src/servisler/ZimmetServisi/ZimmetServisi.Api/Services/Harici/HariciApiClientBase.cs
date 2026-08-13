using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZimmetServisi.Api.Sabitler;

namespace ZimmetServisi.Api.Services.Harici;

public abstract class HariciApiClientBase(HttpClient httpClient)
{
    protected static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected async Task<HariciApiSonucu<T>> GetAsync<T>(string adres, string bearerToken, CancellationToken cancellationToken)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, adres);
            TokenEkle(istek, bearerToken);

            using var cevap = await httpClient.SendAsync(istek, cancellationToken);
            return await CevabiOku<T>(cevap, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServiseUlasilamadi);
        }
        catch (TaskCanceledException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServisZamanindaCevapVermedi);
        }
        catch (JsonException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServisBeklenmeyenFormattaCevapDondu);
        }
    }

    protected async Task<HariciApiSonucu<T>> PostAsync<T>(string adres, object govde, string bearerToken, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(govde, JsonAyarlari);
            using var istek = new HttpRequestMessage(HttpMethod.Post, adres)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            TokenEkle(istek, bearerToken);

            using var cevap = await httpClient.SendAsync(istek, cancellationToken);
            return await CevabiOku<T>(cevap, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServiseUlasilamadi);
        }
        catch (TaskCanceledException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServisZamanindaCevapVermedi);
        }
        catch (JsonException)
        {
            return HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServisBeklenmeyenFormattaCevapDondu);
        }
    }

    private static void TokenEkle(HttpRequestMessage istek, string bearerToken)
    {
        istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    private static async Task<HariciApiSonucu<T>> CevabiOku<T>(HttpResponseMessage cevap, CancellationToken cancellationToken)
    {
        var icerik = await cevap.Content.ReadAsStringAsync(cancellationToken);
        if (cevap.IsSuccessStatusCode)
        {
            var veri = JsonSerializer.Deserialize<T>(icerik, JsonAyarlari);
            return veri is null
                ? HariciApiSonucu<T>.Basarisiz(ZimmetMesajlari.BagimliServisBosCevapDondu)
                : HariciApiSonucu<T>.Basarili(veri);
        }

        return HariciApiSonucu<T>.Basarisiz(HataMesajiniOku(cevap.StatusCode, icerik));
    }

    private static string HataMesajiniOku(HttpStatusCode durumKodu, string icerik)
    {
        var servisMesaji = ServisHataMesajiniOku(icerik);
        if (!string.IsNullOrWhiteSpace(servisMesaji))
        {
            return servisMesaji;
        }

        return durumKodu switch
        {
            HttpStatusCode.Unauthorized => ZimmetMesajlari.BagimliServisOturumuDogrulayamadi,
            HttpStatusCode.Forbidden => ZimmetMesajlari.BagimliServisYetkiVermedi,
            HttpStatusCode.NotFound => ZimmetMesajlari.BagimliServisteKayitBulunamadi,
            HttpStatusCode.BadRequest => ZimmetMesajlari.BagimliServisBilgileriGecerliBulmadi,
            _ => ZimmetMesajlari.BagimliServisHataDondurdu
        };
    }

    private static string? ServisHataMesajiniOku(string icerik)
    {
        if (string.IsNullOrWhiteSpace(icerik))
        {
            return null;
        }

        try
        {
            using var belge = JsonDocument.Parse(icerik);
            if (belge.RootElement.TryGetProperty("hata", out var hata))
            {
                return hata.GetString();
            }

            if (belge.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString();
            }

            if (belge.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString();
            }
        }
        catch (JsonException)
        {
            return icerik;
        }

        return icerik;
    }
}
