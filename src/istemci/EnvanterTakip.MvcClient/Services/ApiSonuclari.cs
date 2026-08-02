namespace EnvanterTakip.MvcClient.Services;

public sealed record ApiIslemSonucu<T>(bool BasariliMi, T? Veri, string? Hata)
{
    public static ApiIslemSonucu<T> Basarili(T veri) => new(true, veri, null);

    public static ApiIslemSonucu<T> Basarisiz(string hata) => new(false, default, hata);
}

public sealed record ApiListeSonucu<T>(bool BasariliMi, IReadOnlyCollection<T> Veri, string? Hata)
{
    public static ApiListeSonucu<T> Basarili(IReadOnlyCollection<T> veri) => new(true, veri, null);

    public static ApiListeSonucu<T> Basarisiz(string hata) => new(false, [], hata);
}
