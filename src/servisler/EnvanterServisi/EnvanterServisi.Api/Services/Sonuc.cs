namespace EnvanterServisi.Api.Services;

public sealed class Sonuc<T>
{
    private Sonuc(bool basariliMi, T? veri, string? hata)
    {
        BasariliMi = basariliMi;
        Veri = veri;
        Hata = hata;
    }

    public bool BasariliMi { get; }
    public T? Veri { get; }
    public string? Hata { get; }

    public static Sonuc<T> Basarili(T veri) => new(true, veri, null);
    public static Sonuc<T> Basarisiz(string hata) => new(false, default, hata);
}
