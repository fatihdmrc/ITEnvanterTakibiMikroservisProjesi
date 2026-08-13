using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;

namespace EnvanterTakip.MvcClient.Services;

public sealed class ZimmetApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<ApiListeSonucu<ZimmetModel>> ZimmetleriListeleAsync(string? token)
        => GetListeAsync<ZimmetModel>("/api/zimmetler", token);

    public Task<ApiListeSonucu<ZimmetModel>> BenimZimmetlerimiListeleAsync(string? token)
        => GetListeAsync<ZimmetModel>("/api/zimmetler/benim", token);

    public Task<ApiIslemSonucu<ZimmetModel>> ZimmetGetirAsync(Guid id, string? token)
        => GetAsync<ZimmetModel>($"/api/zimmetler/{id}", token);

    public Task<ApiIslemSonucu<ZimmetModel>> ZimmetOlusturAsync(ZimmetOlusturFormModel form, string? token)
        => PostAsync<ZimmetModel>("/api/zimmetler", new
        {
            form.CihazId,
            form.PersonelId,
            form.ZimmetTarihi
        }, token);

    public Task<ApiIslemSonucu<ZimmetModel>> IadeAlindiAsync(ZimmetIadeAlindiFormModel form, string? token)
        => PostAsync<ZimmetModel>($"/api/zimmetler/{form.Id}/iade-alindi", new
        {
            form.IadeTarihi,
            form.IadeNotu
        }, token);

    public Task<ApiIslemSonucu<ZimmetModel>> IadeKontroluTamamlaAsync(ZimmetIadeKontroluFormModel form, string? token)
        => PostAsync<ZimmetModel>($"/api/zimmetler/{form.Id}/iade-kontrolu", new
        {
            form.IadeKontrolDurumu,
            form.IadeNotu
        }, token);

    private async Task<ApiIslemSonucu<T>> GetAsync<T>(string adres, string? token)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, adres);
            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            return await CevabiOku<T>(cevap);
        }
        catch (HttpRequestException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ZimmetServisineUlasilamadi);
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ServisZamanindaCevapVermedi);
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ServisBeklenmeyenFormattaCevapDondu);
        }
    }

    private async Task<ApiListeSonucu<T>> GetListeAsync<T>(string adres, string? token)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, adres);
            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            if (!cevap.IsSuccessStatusCode)
            {
                var hataIcerigi = await cevap.Content.ReadAsStringAsync();
                return ApiListeSonucu<T>.Basarisiz(HataMesajiniOku(cevap.StatusCode, hataIcerigi));
            }

            var icerik = await cevap.Content.ReadAsStringAsync();
            var veri = JsonSerializer.Deserialize<IReadOnlyCollection<T>>(icerik, JsonAyarlari) ?? [];
            return ApiListeSonucu<T>.Basarili(veri);
        }
        catch (HttpRequestException)
        {
            return ApiListeSonucu<T>.Basarisiz(MvcMesajlari.ZimmetServisineUlasilamadi);
        }
        catch (TaskCanceledException)
        {
            return ApiListeSonucu<T>.Basarisiz(MvcMesajlari.ServisZamanindaCevapVermedi);
        }
        catch (JsonException)
        {
            return ApiListeSonucu<T>.Basarisiz(MvcMesajlari.ServisBeklenmeyenFormattaCevapDondu);
        }
    }

    private Task<ApiIslemSonucu<T>> PostAsync<T>(string adres, object govde, string? token)
        => SendAsync<T>(HttpMethod.Post, adres, govde, token);

    private async Task<ApiIslemSonucu<T>> SendAsync<T>(HttpMethod method, string adres, object govde, string? token)
    {
        try
        {
            var json = JsonSerializer.Serialize(govde, JsonAyarlari);
            using var istek = new HttpRequestMessage(method, adres)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            return await CevabiOku<T>(cevap);
        }
        catch (HttpRequestException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ZimmetServisineUlasilamadi);
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ServisZamanindaCevapVermedi);
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ServisBeklenmeyenFormattaCevapDondu);
        }
    }

    private static void TokenEkle(HttpRequestMessage istek, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<ApiIslemSonucu<T>> CevabiOku<T>(HttpResponseMessage cevap)
    {
        var icerik = await cevap.Content.ReadAsStringAsync();
        if (cevap.IsSuccessStatusCode)
        {
            var veri = JsonSerializer.Deserialize<T>(icerik, JsonAyarlari);
            return veri is null
                ? ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.ServisBosCevapDondu)
                : ApiIslemSonucu<T>.Basarili(veri);
        }

        return ApiIslemSonucu<T>.Basarisiz(HataMesajiniOku(cevap.StatusCode, icerik));
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
            HttpStatusCode.Unauthorized => MvcMesajlari.OturumBulunamadiVeyaSuresiDoldu,
            HttpStatusCode.Forbidden => MvcMesajlari.YetkiYok,
            HttpStatusCode.NotFound => MvcMesajlari.IstenenKayitBulunamadi,
            HttpStatusCode.BadRequest => MvcMesajlari.GonderilenBilgilerGecerliDegil,
            _ => MvcMesajlari.ServisHataDondurdu
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
