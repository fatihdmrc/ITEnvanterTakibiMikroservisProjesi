using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvanterTakip.MvcClient.Models;

namespace EnvanterTakip.MvcClient.Services;

public sealed class KimlikPersonelApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonAyarlari = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<ApiIslemSonucu<GirisCevapModel>> GirisYapAsync(GirisFormModel form)
    {
        return PostAsync<GirisCevapModel>("/api/kimlik/giris", new
        {
            form.KullaniciAdi,
            form.Sifre
        });
    }

    public Task<ApiListeSonucu<DepartmanModel>> DepartmanlariListeleAsync(string? token)
        => GetListeAsync<DepartmanModel>("/api/departmanlar", token);

    public Task<ApiIslemSonucu<DepartmanModel>> DepartmanOlusturAsync(DepartmanOlusturFormModel form, string? token)
    {
        return PostAsync<DepartmanModel>("/api/departmanlar", new
        {
            form.Ad,
            form.SorumluPersonelId
        }, token);
    }

    public Task<ApiIslemSonucu<DepartmanModel>> DepartmanGuncelleAsync(DepartmanGuncelleFormModel form, string? token)
    {
        return PutAsync<DepartmanModel>($"/api/departmanlar/{form.Id}", new
        {
            form.Ad,
            form.SorumluPersonelId,
            form.AktifMi
        }, token);
    }

    public Task<ApiListeSonucu<PersonelModel>> PersonelleriListeleAsync(string? token)
        => GetListeAsync<PersonelModel>("/api/personeller", token);

    public Task<ApiIslemSonucu<PersonelModel>> PersonelGetirAsync(Guid personelId, string? token)
        => GetAsync<PersonelModel>($"/api/personeller/{personelId}", token);

    public Task<ApiIslemSonucu<PersonelModel>> PersonelOlusturAsync(PersonelOlusturFormModel form, string? token)
    {
        return PostAsync<PersonelModel>("/api/personeller", new
        {
            form.Ad,
            form.Soyad,
            form.Email,
            form.DepartmanId,
            form.Unvan,
            form.DepartmanSorumlusuMu,
            form.IseGirisTarihi
        }, token);
    }

    public Task<ApiIslemSonucu<PersonelModel>> PersonelGuncelleAsync(PersonelGuncelleFormModel form, string? token)
    {
        return PutAsync<PersonelModel>($"/api/personeller/{form.Id}", new
        {
            form.Ad,
            form.Soyad,
            form.Email,
            form.DepartmanId,
            form.Unvan,
            form.DepartmanSorumlusuMu,
            form.Durum,
            form.AktifMi
        }, token);
    }

    public Task<ApiIslemSonucu<PersonelModel>> PersoneliIstenAyrildiYapAsync(Guid personelId, string? token)
    {
        return PostAsync<PersonelModel>($"/api/personeller/{personelId}/isten-ayrildi", new { }, token);
    }

    public Task<ApiListeSonucu<KullaniciModel>> KullanicilariListeleAsync(string? token)
        => GetListeAsync<KullaniciModel>("/api/kullanicilar", token);

    public Task<ApiIslemSonucu<KullaniciModel>> KullaniciOlusturAsync(KullaniciOlusturFormModel form, string? token)
    {
        return PostAsync<KullaniciModel>("/api/kullanicilar", new
        {
            form.KullaniciAdi,
            form.Sifre,
            form.Rol,
            form.PersonelId
        }, token);
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
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisi beklenmeyen formatta cevap döndürdü.");
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
            return ApiListeSonucu<T>.Basarisiz("Kimlik ve personel servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiListeSonucu<T>.Basarisiz("Kimlik ve personel servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiListeSonucu<T>.Basarisiz("Kimlik ve personel servisi beklenmeyen formatta cevap döndürdü.");
        }
    }

    private Task<ApiIslemSonucu<T>> PostAsync<T>(string adres, object govde, string? token = null)
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
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisine ulaşılamadı. Servisin çalıştığından emin ol.");
        }
        catch (TaskCanceledException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisi zamanında cevap vermedi.");
        }
        catch (JsonException)
        {
            return ApiIslemSonucu<T>.Basarisiz("Kimlik ve personel servisi beklenmeyen formatta cevap döndürdü.");
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
