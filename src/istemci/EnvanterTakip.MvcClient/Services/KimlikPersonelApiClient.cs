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

    public async Task<IReadOnlyCollection<DepartmanModel>> DepartmanlariListeleAsync(string? token)
    {
        return await GetListeAsync<DepartmanModel>("/api/departmanlar", token);
    }

    public Task<ApiIslemSonucu<DepartmanModel>> DepartmanOlusturAsync(DepartmanOlusturFormModel form, string? token)
    {
        return PostAsync<DepartmanModel>("/api/departmanlar", new
        {
            form.Ad,
            form.SorumluPersonelId
        }, token);
    }

    public async Task<IReadOnlyCollection<PersonelModel>> PersonelleriListeleAsync(string? token)
    {
        return await GetListeAsync<PersonelModel>("/api/personeller", token);
    }

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

    public Task<ApiIslemSonucu<PersonelModel>> PersoneliIstenAyrildiYapAsync(Guid personelId, string? token)
    {
        return PostAsync<PersonelModel>($"/api/personeller/{personelId}/isten-ayrildi", new { }, token);
    }

    public async Task<IReadOnlyCollection<KullaniciModel>> KullanicilariListeleAsync(string? token)
    {
        return await GetListeAsync<KullaniciModel>("/api/kullanicilar", token);
    }

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

    private async Task<ApiIslemSonucu<T>> PostAsync<T>(string adres, object govde, string? token = null)
    {
        var json = JsonSerializer.Serialize(govde, JsonAyarlari);
        using var istek = new HttpRequestMessage(HttpMethod.Post, adres)
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

        var hata = HataMesajiniOku(icerik);
        return ApiIslemSonucu<T>.Basarisiz(hata);
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

public sealed record ApiIslemSonucu<T>(bool BasariliMi, T? Veri, string? Hata)
{
    public static ApiIslemSonucu<T> Basarili(T veri) => new(true, veri, null);

    public static ApiIslemSonucu<T> Basarisiz(string hata) => new(false, default, hata);
}
