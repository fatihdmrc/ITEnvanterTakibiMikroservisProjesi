namespace EnvanterServisi.Api.Sabitler;

public static class EnvanterMesajlari
{
    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";

    public const string KategoriAdiZorunlu = "Kategori adı zorunludur.";
    public const string UstKategoriBulunamadi = "Üst kategori bulunamadı.";
    public const string KategoriAdiKullaniliyor = "Aynı üst kategori altında bu ada sahip kategori zaten var.";
    public const string KategoriBulunamadi = "Kategori bulunamadı.";
    public const string KategoriKendiUstuOlamaz = "Kategori kendi üst kategorisi olamaz.";

    public const string LokasyonAdiZorunlu = "Lokasyon adı zorunludur.";
    public const string UstLokasyonBulunamadi = "Üst lokasyon bulunamadı.";
    public const string LokasyonAdiKullaniliyor = "Aynı üst lokasyon altında bu ada sahip lokasyon zaten var.";
    public const string LokasyonBulunamadi = "Lokasyon bulunamadı.";
    public const string LokasyonKendiUstuOlamaz = "Lokasyon kendi üst lokasyonu olamaz.";

    public const string CihazBulunamadi = "Cihaz bulunamadı.";
    public const string CihazKimligiZorunlu = "Seri numarası veya asset tag alanlarından en az biri zorunludur.";
    public const string CihazKimligiKullaniliyor = "Seri numarası veya asset tag başka bir cihazda kullanılıyor.";
    public const string CihazDurumHareketiDesteklenmiyor = "Bu neden cihaz durum hareketi için desteklenmiyor.";
    public const string SeriNumaraliCihazAktifKategoriYok = "Seri numaralı cihaz için aktif kategori bulunamadı.";

    public const string SarfMalzemeAdiZorunlu = "Sarf malzeme adı zorunludur.";
    public const string SarfMalzemeBulunamadi = "Sarf malzeme bulunamadı.";
    public const string SarfMalzemeAdiKullaniliyor = "Aynı kategori ve lokasyonda bu sarf malzeme zaten var.";
    public const string MiktarVeKritikStokNegatifOlamaz = "Miktar ve kritik stok seviyesi negatif olamaz.";
    public const string SarfStokHareketiDesteklenmiyor = "Bu neden sarf malzeme stok hareketi için desteklenmiyor.";
    public const string StokHareketMiktariPozitifOlmali = "Stok hareket miktarı sıfırdan büyük olmalıdır.";
    public const string EldekiMiktardanFazlaCikisYapilamaz = "Eldeki miktardan fazla stok çıkışı yapılamaz.";
    public const string SarfMalzemeAktifKategoriYok = "Sarf malzeme için aktif kategori bulunamadı.";

    public const string KritikStokKuraliBulunamadi = "Kritik stok kuralı bulunamadı.";
    public const string KritikStokNegatifOlamaz = "Kritik stok seviyesi negatif olamaz.";
    public const string AktifLokasyonBulunamadi = "Aktif lokasyon bulunamadı.";
    public const string AktifKategoriBulunamadi = "Aktif kategori bulunamadı.";
    public const string VarsayilanBirim = "Adet";

    public const string TokenKullaniciIdYok = "Token içinde KullaniciId bilgisi bulunamadı.";
    public const string EnvanterDbBaglantisiYok = "Envanter veritabanı bağlantısı bulunamadı.";
    public const string JwtAyarlariYok = "JWT ayarları bulunamadı.";
    public const string DenetimServisiAdresiYok = "Denetim kaydı servisi adresi tanımlı değil.";
    public const string CrudDenetimAciklamasi = "{0} {1} işlemi başarıyla tamamlandı.";
}
