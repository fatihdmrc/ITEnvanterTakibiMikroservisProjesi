using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterTakip.MvcClient.Models;

namespace EnvanterTakip.MvcClient.Services;

public sealed class EnvanterApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<IReadOnlyCollection<KategoriModel>> KategorileriListeleAsync(string? token)
        => GetListeAsync<KategoriModel>("/api/kategoriler", token);

    public Task<ApiIslemSonucu<KategoriModel>> KategoriOlusturAsync(KategoriOlusturFormModel form, string? token)
        => PostAsync<KategoriModel>("/api/kategoriler", new
        {
            form.Ad,
            form.UstKategoriId,
            form.VarlikTuru,
            form.KritikStokSeviyesi
        }, token);

    public Task<ApiIslemSonucu<KategoriModel>> KategoriGuncelleAsync(KategoriGuncelleFormModel form, string? token)
        => PutAsync<KategoriModel>($"/api/kategoriler/{form.Id}", new
        {
            form.Ad,
            form.UstKategoriId,
            form.VarlikTuru,
            form.KritikStokSeviyesi,
            form.AktifMi
        }, token);

    public Task<IReadOnlyCollection<LokasyonModel>> LokasyonlariListeleAsync(string? token)
        => GetListeAsync<LokasyonModel>("/api/lokasyonlar", token);

    public Task<ApiIslemSonucu<LokasyonModel>> LokasyonOlusturAsync(LokasyonOlusturFormModel form, string? token)
        => PostAsync<LokasyonModel>("/api/lokasyonlar", new
        {
            form.Ad,
            form.UstLokasyonId
        }, token);

    public Task<ApiIslemSonucu<LokasyonModel>> LokasyonGuncelleAsync(LokasyonGuncelleFormModel form, string? token)
        => PutAsync<LokasyonModel>($"/api/lokasyonlar/{form.Id}", new
        {
            form.Ad,
            form.UstLokasyonId,
            form.AktifMi
        }, token);

    public Task<IReadOnlyCollection<CihazModel>> CihazlariListeleAsync(string? token)
        => GetListeAsync<CihazModel>("/api/cihazlar", token);

    public Task<ApiIslemSonucu<CihazModel>> CihazOlusturAsync(CihazOlusturFormModel form, string? token)
        => PostAsync<CihazModel>("/api/cihazlar", new
        {
            form.SeriNumarasi,
            form.AssetTag,
            form.Ad,
            form.Marka,
            form.Model,
            form.KategoriId,
            form.LokasyonId,
            form.EnvantereGirisTarihi
        }, token);

    public Task<ApiIslemSonucu<CihazModel>> CihazGuncelleAsync(CihazGuncelleFormModel form, string? token)
        => PutAsync<CihazModel>($"/api/cihazlar/{form.Id}", new
        {
            form.SeriNumarasi,
            form.AssetTag,
            form.Ad,
            form.Marka,
            form.Model,
            form.KategoriId,
            form.LokasyonId,
            form.Durum,
            form.EnvantereGirisTarihi,
            form.EnvanterdenCikisTarihi,
            form.EldenCikarmaTipi,
            form.EldenCikarmaAciklamasi,
            form.SatilanKisiVeyaKurum,
            form.AktifMi,
            form.ToplamVarligaDahilMi
        }, token);

    public Task<ApiIslemSonucu<CihazModel>> CihazStokHareketiIsleAsync(CihazStokHareketiFormModel form, string? token)
        => PostAsync<CihazModel>($"/api/cihazlar/{form.Id}/stok-hareketleri", new
        {
            form.Neden,
            form.Aciklama,
            form.EldenCikarmaTipi,
            form.SatilanKisiVeyaKurum
        }, token);

    public Task<IReadOnlyCollection<SarfMalzemeModel>> SarfMalzemeleriListeleAsync(string? token)
        => GetListeAsync<SarfMalzemeModel>("/api/sarf-malzemeler", token);

    public Task<ApiIslemSonucu<SarfMalzemeModel>> SarfMalzemeOlusturAsync(SarfMalzemeOlusturFormModel form, string? token)
        => PostAsync<SarfMalzemeModel>("/api/sarf-malzemeler", new
        {
            form.Ad,
            form.KategoriId,
            form.LokasyonId,
            form.EldekiMiktar,
            form.KritikStokSeviyesi,
            form.Birim
        }, token);

    public Task<ApiIslemSonucu<SarfMalzemeModel>> SarfMalzemeGuncelleAsync(SarfMalzemeGuncelleFormModel form, string? token)
        => PutAsync<SarfMalzemeModel>($"/api/sarf-malzemeler/{form.Id}", new
        {
            form.Ad,
            form.KategoriId,
            form.LokasyonId,
            form.EldekiMiktar,
            form.KritikStokSeviyesi,
            form.Birim,
            form.AktifMi
        }, token);

    public Task<ApiIslemSonucu<SarfMalzemeModel>> SarfMalzemeStokHareketiIsleAsync(SarfMalzemeStokHareketiFormModel form, string? token)
        => PostAsync<SarfMalzemeModel>($"/api/sarf-malzemeler/{form.Id}/stok-hareketleri", new
        {
            form.HareketTipi,
            form.Neden,
            form.Miktar,
            form.Aciklama
        }, token);

    public async Task<StokOzetModel> StokOzetiniGetirAsync(string? token)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, "/api/stok/ozet");
            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            if (!cevap.IsSuccessStatusCode)
            {
                return new StokOzetModel(0, 0, 0, []);
            }

            var icerik = await cevap.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StokOzetModel>(icerik, JsonAyarlari)
                ?? new StokOzetModel(0, 0, 0, []);
        }
        catch (HttpRequestException)
        {
            return new StokOzetModel(0, 0, 0, []);
        }
    }

    private async Task<IReadOnlyCollection<T>> GetListeAsync<T>(string adres, string? token)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, adres);
            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            if (!cevap.IsSuccessStatusCode)
            {
                return [];
            }

            var icerik = await cevap.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IReadOnlyCollection<T>>(icerik, JsonAyarlari) ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    private Task<ApiIslemSonucu<T>> PostAsync<T>(string adres, object govde, string? token)
        => SendAsync<T>(HttpMethod.Post, adres, govde, token);

    private Task<ApiIslemSonucu<T>> PutAsync<T>(string adres, object govde, string? token)
        => SendAsync<T>(HttpMethod.Put, adres, govde, token);

    private async Task<ApiIslemSonucu<T>> SendAsync<T>(HttpMethod method, string adres, object govde, string? token)
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
                ? ApiIslemSonucu<T>.Basarisiz("Servis boş cevap döndürdü.")
                : ApiIslemSonucu<T>.Basarili(veri);
        }

        return ApiIslemSonucu<T>.Basarisiz(HataMesajiniOku(icerik));
    }

    private static string HataMesajiniOku(string icerik)
    {
        if (string.IsNullOrWhiteSpace(icerik))
        {
            return "Servis hata döndürdü.";
        }

        using var belge = JsonDocument.Parse(icerik);
        return belge.RootElement.TryGetProperty("hata", out var hata)
            ? hata.GetString() ?? "Servis hata döndürdü."
            : icerik;
    }
}
