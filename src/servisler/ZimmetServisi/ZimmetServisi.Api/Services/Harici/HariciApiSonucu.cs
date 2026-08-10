namespace ZimmetServisi.Api.Services.Harici;

public sealed record HariciApiSonucu<T>(bool BasariliMi, T? Veri, string? Hata)
{
    public static HariciApiSonucu<T> Basarili(T veri) => new(true, veri, null);
    public static HariciApiSonucu<T> Basarisiz(string hata) => new(false, default, hata);
}
