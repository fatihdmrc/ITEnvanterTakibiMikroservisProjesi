using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterTakip.MvcClient.Models;
using EnvanterTakip.MvcClient.Sabitler;

namespace EnvanterTakip.MvcClient.Services;

public sealed class DenetimApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<ApiIslemSonucu<DenetimListeCevapModel>> DenetimKayitlariniListeleAsync(DenetimFiltreModel filtre, string? token)
    {
        var parametreler = new List<KeyValuePair<string, string>>();

        if (filtre.KayitTuru.HasValue) parametreler.Add(new("kayitTuru", filtre.KayitTuru.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(filtre.EventAdi)) parametreler.Add(new("eventAdi", filtre.EventAdi));
        if (!string.IsNullOrWhiteSpace(filtre.IslemTuru)) parametreler.Add(new("islemTuru", filtre.IslemTuru));
        if (!string.IsNullOrWhiteSpace(filtre.KaynakServis)) parametreler.Add(new("kaynakServis", filtre.KaynakServis));
        if (!string.IsNullOrWhiteSpace(filtre.VarlikTuru)) parametreler.Add(new("varlikTuru", filtre.VarlikTuru));
        if (!string.IsNullOrWhiteSpace(filtre.VarlikId)) parametreler.Add(new("varlikId", filtre.VarlikId));
        if (filtre.KullaniciId.HasValue) parametreler.Add(new("kullaniciId", filtre.KullaniciId.Value.ToString()));
        if (filtre.Baslangic.HasValue) parametreler.Add(new("baslangic", filtre.Baslangic.Value.ToString("O")));
        if (filtre.Bitis.HasValue) parametreler.Add(new("bitis", filtre.Bitis.Value.ToString("O")));

        parametreler.Add(new("sayfa", Math.Max(filtre.Sayfa, 1).ToString()));
        parametreler.Add(new("sayfaBoyutu", Math.Clamp(filtre.SayfaBoyutu, 1, 100).ToString()));

        return GetAsync<DenetimListeCevapModel>(QueryStringEkle("/api/denetim-kayitlari", parametreler), token);
    }

    public Task<ApiIslemSonucu<DenetimKaydiModel>> DenetimKaydiGetirAsync(string id, string? token)
        => GetAsync<DenetimKaydiModel>($"/api/denetim-kayitlari/{Uri.EscapeDataString(id)}", token);

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
            return ApiIslemSonucu<T>.Basarisiz(MvcMesajlari.DenetimServisineUlasilamadi);
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
            if (belge.RootElement.TryGetProperty("hata", out var hata)) return hata.GetString();
            if (belge.RootElement.TryGetProperty("title", out var title)) return title.GetString();
            if (belge.RootElement.TryGetProperty("detail", out var detail)) return detail.GetString();
        }
        catch (JsonException)
        {
            return icerik;
        }

        return icerik;
    }

    private static void TokenEkle(HttpRequestMessage istek, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static string QueryStringEkle(string adres, IReadOnlyCollection<KeyValuePair<string, string>> parametreler)
    {
        if (parametreler.Count == 0)
        {
            return adres;
        }

        var query = string.Join("&", parametreler.Select(parametre =>
            $"{Uri.EscapeDataString(parametre.Key)}={Uri.EscapeDataString(parametre.Value)}"));

        return $"{adres}?{query}";
    }
}
