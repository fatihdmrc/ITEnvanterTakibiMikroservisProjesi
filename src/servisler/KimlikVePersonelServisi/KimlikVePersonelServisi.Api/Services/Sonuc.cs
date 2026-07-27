namespace KimlikVePersonelServisi.Api.Services;

public sealed record Sonuc<T>(bool BasariliMi, T? Veri, string? Hata)
{
    public static Sonuc<T> Basarili(T veri) => new(true, veri, null);

    public static Sonuc<T> Basarisiz(string hata) => new(false, default, hata);
}
