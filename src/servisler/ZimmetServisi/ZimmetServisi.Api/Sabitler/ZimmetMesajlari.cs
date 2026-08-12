namespace ZimmetServisi.Api.Sabitler;

public static class ZimmetMesajlari
{
    public const string CihazVePersonelSecimiZorunlu = "Cihaz ve personel seçimi zorunludur.";
    public const string CihazdaAcikZimmetVar = "Bu cihaz için açık bir zimmet zaten var.";
    public const string AktifOlmayanPersoneleZimmetOlusturulamaz = "Aktif olmayan veya işten ayrılmış personele zimmet oluşturulamaz.";
    public const string KullanilabilirOlmayanCihazZimmetlenemez = "Yalnızca aktif ve kullanılabilir durumdaki cihazlar zimmetlenebilir.";
    public const string ZimmetKaydiOlusturulamadi = "Zimmet kaydı oluşturulamadı. Bu cihaz için açık zimmet olup olmadığını kontrol et.";
    public const string ZimmetKaydiBulunamadi = "Zimmet kaydı bulunamadı.";
    public const string SadeceAktifZimmetIadeSurecineAlinir = "Yalnızca aktif zimmetler iade sürecine alınabilir.";
    public const string SadeceIadeSurecindekiZimmetKontrolEdilir = "Fiziki kontrol yalnızca iade sürecindeki zimmetler için tamamlanabilir.";

    public static string PersonelDogrulanamadi(string? hata)
        => $"Personel doğrulanamadı: {hata}";

    public static string CihazDogrulanamadi(string? hata)
        => $"Cihaz doğrulanamadı: {hata}";

    public static string CihazDurumuZimmetliYapilamadi(string? hata)
        => $"Cihaz durumu zimmetli yapılamadı: {hata}";

    public static string CihazIadeIncelemesineAlinamadi(string? hata)
        => $"Cihaz iade incelemesine alınamadı: {hata}";

    public static string CihazDurumuIadeKontroluneGoreGuncellenemedi(string? hata)
        => $"Cihaz durumu iade kontrol sonucuna göre güncellenemedi: {hata}";

    public static string ZimmetlendiAciklamasi(string personelAdSoyad)
        => $"{personelAdSoyad} personeline zimmetlendi.";

    public static string ZimmetIadesiAlindiAciklamasi(string personelAdSoyad)
        => $"{personelAdSoyad} personelinden zimmet iadesi alındı.";
}
