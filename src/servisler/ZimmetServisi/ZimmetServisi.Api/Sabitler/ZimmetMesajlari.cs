namespace ZimmetServisi.Api.Sabitler;

public static class ZimmetMesajlari
{
    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";

    public const string CihazVePersonelSecimiZorunlu = "Cihaz ve personel seçimi zorunludur.";
    public const string CihazdaAcikZimmetVar = "Bu cihaz için açık bir zimmet zaten var.";
    public const string AktifOlmayanPersoneleZimmetOlusturulamaz = "Aktif olmayan veya işten ayrılmış personele zimmet oluşturulamaz.";
    public const string KullanilabilirOlmayanCihazZimmetlenemez = "Yalnızca aktif ve kullanılabilir durumdaki cihazlar zimmetlenebilir.";
    public const string ZimmetKaydiOlusturulamadi = "Zimmet kaydı oluşturulamadı. Bu cihaz için açık zimmet olup olmadığını kontrol et.";
    public const string ZimmetKaydiBulunamadi = "Zimmet kaydı bulunamadı.";
    public const string SadeceAktifZimmetIadeSurecineAlinir = "Yalnızca aktif zimmetler iade sürecine alınabilir.";
    public const string SadeceIadeSurecindekiZimmetKontrolEdilir = "Fiziki kontrol yalnızca iade sürecindeki zimmetler için tamamlanabilir.";

    public const string BagimliServiseUlasilamadi = "Bağımlı servise ulaşılamadı.";
    public const string BagimliServisZamanindaCevapVermedi = "Bağımlı servis zamanında cevap vermedi.";
    public const string BagimliServisBeklenmeyenFormattaCevapDondu = "Bağımlı servis beklenmeyen formatta cevap döndürdü.";
    public const string BagimliServisBosCevapDondu = "Bağımlı servis boş cevap döndürdü.";
    public const string BagimliServisOturumuDogrulayamadi = "Bağımlı servis oturumu doğrulayamadı.";
    public const string BagimliServisYetkiVermedi = "Bağımlı servis bu işlem için yetki vermedi.";
    public const string BagimliServisteKayitBulunamadi = "Bağımlı serviste istenen kayıt bulunamadı.";
    public const string BagimliServisBilgileriGecerliBulmadi = "Bağımlı servis gönderilen bilgileri geçerli bulmadı.";
    public const string BagimliServisHataDondurdu = "Bağımlı servis hata döndürdü.";

    public const string ZimmetDbBaglantisiYok = "Zimmet veritabanı bağlantısı bulunamadı.";
    public const string JwtAyarlariYok = "JWT ayarları bulunamadı.";
    public const string KimlikPersonelServisiAdresiYok = "Kimlik ve personel servisi adresi tanımlı değil.";
    public const string EnvanterServisiAdresiYok = "Envanter servisi adresi tanımlı değil.";
    public const string DenetimServisiAdresiYok = "Denetim kaydı servisi adresi tanımlı değil.";
    public const string TokenPersonelIdYok = "Token içinde PersonelId bilgisi bulunamadı.";
    public const string TokenKullaniciBilgisiYok = "Token içinde kullanıcı bilgisi bulunamadı.";
    public const string CrudDenetimAciklamasi = "{0} {1} işlemi başarıyla tamamlandı.";

    public const string IadeKontrolSonucuSaglam = "sağlam, tekrar kullanılabilir";
    public const string IadeKontrolSonucuBakimda = "bakıma alınacak";
    public const string IadeKontrolSonucuHurdaIskarta = "hurda/ıskarta olarak ayrıldı";
    public const string IadeKontrolSonucuHasarli = "hasarlı teslim alındı";
    public const string IadeKontrolSonucuIncelemede = "incelemede";

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

    public static string IadeKontrolAciklamasi(string personelAdSoyad, string sonuc, string? not)
        => string.IsNullOrWhiteSpace(not)
            ? $"{personelAdSoyad} personelinden alınan zimmetin fiziki kontrol sonucu: {sonuc}."
            : $"{personelAdSoyad} personelinden alınan zimmetin fiziki kontrol sonucu: {sonuc}. Not: {not}";
}
