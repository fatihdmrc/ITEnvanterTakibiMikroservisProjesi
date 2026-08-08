using System.Net;
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

    public Task<ApiListeSonucu<KategoriModel>> KategorileriListeleAsync(string? token)
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

    public Task<ApiListeSonucu<LokasyonModel>> LokasyonlariListeleAsync(string? token)
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

    public Task<ApiListeSonucu<CihazModel>> CihazlariListeleAsync(string? token)
        => CihazlariListeleAsync(new CihazFiltreModel(), token);

    public Task<ApiListeSonucu<CihazModel>> CihazlariListeleAsync(CihazFiltreModel filtre, string? token)
    {
        var parametreler = new List<KeyValuePair<string, string>>();

        if (filtre.KategoriId.HasValue)
        {
            parametreler.Add(new("kategoriId", filtre.KategoriId.Value.ToString()));
        }

        if (filtre.LokasyonId.HasValue)
        {
            parametreler.Add(new("lokasyonId", filtre.LokasyonId.Value.ToString()));
        }

        if (filtre.AktifMi.HasValue)
        {
            parametreler.Add(new("aktifMi", filtre.AktifMi.Value.ToString().ToLowerInvariant()));
        }

        return GetListeAsync<CihazModel>(QueryStringEkle("/api/cihazlar", parametreler), token);
    }

    public Task<ApiIslemSonucu<CihazModel>> CihazGetirAsync(Guid id, string? token)
        => GetAsync<CihazModel>($"/api/cihazlar/{id}", token);

    public Task<ApiIslemSonucu<CihazModel>> CihazOlusturAsync(CihazOlusturFormModel form, string? token)
        => PostAsync<CihazModel>("/api/cihazlar", new
        {
            form.SeriNumarasi,
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
            form.EnvantereGirisTarihi
        }, token);

    public Task<ApiIslemSonucu<CihazModel>> CihazDurumHareketiIsleAsync(CihazDurumHareketiFormModel form, string? token)
        => PostAsync<CihazModel>($"/api/cihazlar/{form.Id}/durum-hareketleri", new
        {
            form.Neden,
            form.Aciklama,
            form.EldenCikarmaTipi,
            form.SatilanKisiVeyaKurum
        }, token);

    public Task<ApiListeSonucu<SarfMalzemeModel>> SarfMalzemeleriListeleAsync(string? token)
        => GetListeAsync<SarfMalzemeModel>("/api/sarf-malzemeler", token);

    public Task<ApiIslemSonucu<SarfMalzemeModel>> SarfMalzemeGetirAsync(Guid id, string? token)
        => GetAsync<SarfMalzemeModel>($"/api/sarf-malzemeler/{id}", token);

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

    public Task<ApiListeSonucu<StokHareketiModel>> StokHareketleriniListeleAsync(Guid? cihazId, Guid? sarfMalzemeId, string? token)
    {
        var parametreler = new List<KeyValuePair<string, string>>();

        if (cihazId.HasValue)
        {
            parametreler.Add(new("cihazId", cihazId.Value.ToString()));
        }

        if (sarfMalzemeId.HasValue)
        {
            parametreler.Add(new("sarfMalzemeId", sarfMalzemeId.Value.ToString()));
        }

        return GetListeAsync<StokHareketiModel>(QueryStringEkle("/api/stok/hareketler", parametreler), token);
    }

    public async Task<ApiIslemSonucu<StokOzetModel>> StokOzetiniGetirAsync(string? token)
    {
        try
        {
            using var istek = new HttpRequestMessage(HttpMethod.Get, "/api/stok/ozet");
            TokenEkle(istek, token);

            using var cevap = await httpClient.SendAsync(istek);
            return await CevabiOku<StokOzetModel>(cevap);
        }
        catch (HttpRequestException)
        {
            return ApiIslemSonucu<StokOzetModel>.Basarisiz("Envanter servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<StokOzetModel>.Basarisiz("Envanter servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<StokOzetModel>.Basarisiz("Envanter servisi beklenmeyen formatta cevap döndürdü.");
        }
    }

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
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisi beklenmeyen formatta cevap döndürdü.");
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
            return ApiListeSonucu<T>.Basarisiz("Envanter servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiListeSonucu<T>.Basarisiz("Envanter servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiListeSonucu<T>.Basarisiz("Envanter servisi beklenmeyen formatta cevap döndürdü.");
        }
    }

    private Task<ApiIslemSonucu<T>> PostAsync<T>(string adres, object govde, string? token)
        => SendAsync<T>(HttpMethod.Post, adres, govde, token);

    private Task<ApiIslemSonucu<T>> PutAsync<T>(string adres, object govde, string? token)
        => SendAsync<T>(HttpMethod.Put, adres, govde, token);

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
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Envanter servisi beklenmeyen formatta cevap döndürdü.");
        }
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
            HttpStatusCode.Unauthorized => "Oturum bulunamadı veya süresi doldu. Lütfen tekrar giriş yap.",
            HttpStatusCode.Forbidden => "Bu işlem için yetkin yok.",
            HttpStatusCode.NotFound => "İstenen kayıt bulunamadı.",
            HttpStatusCode.BadRequest => "Gönderilen bilgiler geçerli değil.",
            _ => "Servis hata döndürdü."
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
