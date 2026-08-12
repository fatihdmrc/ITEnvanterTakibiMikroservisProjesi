namespace EnvanterTakip.MvcClient.Sabitler;

public static class MvcSabitleri
{
    public const string TokenSessionKey = "KimlikToken";
    public const string KullaniciAdiSessionKey = "KullaniciAdi";
    public const string KullaniciIdSessionKey = "KullaniciId";
    public const string PersonelIdSessionKey = "PersonelId";
    public const string RolSessionKey = "Rol";

    public const string BasariMesajiTempDataKey = "BasariMesaji";
    public const string HataMesajiTempDataKey = "HataMesaji";

    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";
}

public static class MvcMesajlari
{
    public const string OturumYok = "Bu işlem için önce giriş yapmalısın.";
    public const string KontrolPanelindenOturumYok = "Bu işlem için önce kontrol panelinden giriş yapmalısın.";
    public const string GirisBilgileriEksik = "Kullanıcı adı ve şifre girilmelidir.";
    public const string OturumKapatildi = "Oturum kapatıldı.";

    public const string DepartmanBilgileriHatali = "Departman bilgileri eksik veya hatalı.";
    public const string DepartmanGuncellemeBilgileriHatali = "Departman güncelleme bilgileri eksik veya hatalı.";
    public const string DepartmanOlusturuldu = "Departman oluşturuldu.";
    public const string DepartmanGuncellendi = "Departman güncellendi.";
    public const string DepartmanPasiflestirildi = "Departman pasifleştirildi.";

    public const string PersonelBilgileriHatali = "Personel bilgileri eksik veya hatalı.";
    public const string PersonelGuncellemeBilgileriHatali = "Personel güncelleme bilgileri eksik veya hatalı.";
    public const string PersonelOlusturuldu = "Personel oluşturuldu.";
    public const string KullaniciBilgileriHatali = "Kullanıcı bilgileri eksik veya hatalı.";
    public const string KullaniciOlusturuldu = "Kullanıcı oluşturuldu.";

    public const string KategoriBilgileriHatali = "Kategori bilgileri eksik veya hatalı.";
    public const string KategoriGuncellemeBilgileriHatali = "Kategori güncelleme bilgileri eksik veya hatalı.";
    public const string KategoriOlusturuldu = "Kategori oluşturuldu.";
    public const string KategoriGuncellendi = "Kategori güncellendi.";
    public const string KategoriPasiflestirildi = "Kategori pasifleştirildi.";
    public const string LokasyonBilgileriHatali = "Lokasyon bilgileri eksik veya hatalı.";
    public const string LokasyonGuncellemeBilgileriHatali = "Lokasyon güncelleme bilgileri eksik veya hatalı.";
    public const string LokasyonOlusturuldu = "Lokasyon oluşturuldu.";
    public const string LokasyonGuncellendi = "Lokasyon güncellendi.";
    public const string LokasyonPasiflestirildi = "Lokasyon pasifleştirildi.";
    public const string CihazBilgileriHatali = "Cihaz bilgileri eksik veya hatalı.";
    public const string CihazGuncellemeBilgileriHatali = "Cihaz güncelleme bilgileri eksik veya hatalı.";
    public const string CihazGuncellenemedi = "Cihaz güncellenemedi.";
    public const string CihazOlusturuldu = "Cihaz oluşturuldu.";
    public const string CihazGuncellendi = "Cihaz güncellendi.";
    public const string CihazDurumHareketiHatali = "Cihaz durum hareketi bilgileri eksik veya hatalı.";
    public const string CihazDurumHareketiIslendi = "Cihaz durum hareketi işlendi.";
    public const string SarfMalzemeBilgileriHatali = "Sarf malzeme bilgileri eksik veya hatalı.";
    public const string SarfMalzemeGuncellemeBilgileriHatali = "Sarf malzeme güncelleme bilgileri eksik veya hatalı.";
    public const string SarfMalzemeGuncellenemedi = "Sarf malzeme güncellenemedi.";
    public const string SarfMalzemeOlusturuldu = "Sarf malzeme oluşturuldu.";
    public const string SarfMalzemeGuncellendi = "Sarf malzeme güncellendi.";
    public const string SarfMalzemePasiflestirildi = "Sarf malzeme pasifleştirildi.";
    public const string SarfStokHareketiHatali = "Sarf malzeme stok hareketi bilgileri eksik veya hatalı.";
    public const string SarfStokHareketiIslendi = "Sarf malzeme stok hareketi işlendi.";

    public const string ZimmetOlusturmaYetkisiYok = "Zimmet oluşturmak için Admin veya ITPersoneli rolü gerekir.";
    public const string ZimmetOlusturmaBilgileriHatali = "Zimmet oluşturma bilgileri eksik veya hatalı.";
    public const string ZimmetOlusturuldu = "Zimmet oluşturuldu ve cihaz zimmetli duruma alındı.";
    public const string ZimmetIadeYetkisiYok = "Zimmet iadesi almak için Admin veya ITPersoneli rolü gerekir.";
    public const string SadeceAktifZimmetIadeSurecineAlinir = "Yalnızca aktif zimmetler iade sürecine alınabilir.";
    public const string ZimmetIadesiAlindi = "Zimmet iadesi alındı ve cihaz incelemeye alındı.";
    public const string IadeKontrolYetkisiYok = "İade kontrolünü tamamlamak için Admin veya ITPersoneli rolü gerekir.";
    public const string SadeceIadeSurecindekiZimmetKontrolEdilir = "Fiziki kontrol yalnızca iade sürecindeki zimmetler için tamamlanabilir.";
    public const string IadeKontroluTamamlandi = "İade kontrolü tamamlandı ve cihaz durumu güncellendi.";

    public static string GirisYapildi(string kullaniciAdi)
        => $"{kullaniciAdi} kullanıcısı ile giriş yapıldı.";

    public static string PersonelGuncellendi(string ad, string soyad)
        => $"{ad} {soyad} personeli güncellendi.";

    public static string PersonelIstenAyrildi(string adSoyad, string departmanAdi)
        => $"{adSoyad} {departmanAdi} personeli işten ayrıldı yapıldı.";

    public static string ListeAlinamadi(string listeAdi, string? hata)
        => $"{listeAdi} alınamadı: {hata}";
}
