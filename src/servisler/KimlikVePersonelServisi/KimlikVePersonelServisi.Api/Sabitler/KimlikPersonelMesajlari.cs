namespace KimlikVePersonelServisi.Api.Sabitler;

public static class KimlikPersonelMesajlari
{
    public const string AdminRolu = "Admin";
    public const string ITPersoneliRolu = "ITPersoneli";

    public const string DepartmanAdiZorunlu = "Departman adı zorunludur.";
    public const string DepartmanAdiKullaniliyor = "Aynı ada sahip departman zaten var.";
    public const string SorumluPersonelBulunamadi = "Sorumlu personel bulunamadı.";
    public const string DepartmanBulunamadi = "Departman bulunamadı.";

    public const string PersonelAdSoyadZorunlu = "Personel adı ve soyadı zorunludur.";
    public const string GecerliEmailZorunlu = "Geçerli bir e-posta adresi girilmelidir.";
    public const string AktifDepartmanBulunamadi = "Aktif departman bulunamadı.";
    public const string PersonelEmailKullaniliyor = "Bu e-posta adresiyle kayıtlı personel zaten var.";
    public const string PersonelBulunamadi = "Personel bulunamadı.";
    public const string PersonelEmailBaskaPersonelde = "Bu e-posta adresi başka bir personel tarafından kullanılıyor.";

    public const string KullaniciIcinPersonelZorunlu = "Kullanıcı oluşturmak için personel kaydı zorunludur.";
    public const string PasifPersoneleKullaniciOlusturulamaz = "İşten ayrılmış veya pasif personel için kullanıcı oluşturulamaz.";
    public const string GecerliRolZorunlu = "Geçerli bir kullanıcı rolü seçilmelidir.";
    public const string KullaniciAdiKullaniliyor = "Bu kullanıcı adı zaten kullanılıyor.";
    public const string PersonelinKullaniciHesabiVar = "Bu personel için kullanıcı hesabı zaten oluşturulmuş.";
    public const string KullaniciAdiVeyaSifreHatali = "Kullanıcı adı veya şifre hatalı.";
    public const string PersonelAktifDegilGirisYapilamaz = "Personel kaydı aktif olmadığı için giriş yapılamaz.";

    public const string PersonelBulunamadiApi = "Personel bulunamadı.";
    public const string TokenKullaniciBilgisiYok = "Token içinde kullanıcı bilgisi bulunamadı.";
    public const string KimlikDbBaglantisiYok = "Kimlik/personel veritabanı bağlantısı bulunamadı.";
    public const string JwtAyarlariYok = "JWT ayarları bulunamadı.";
    public const string DenetimServisiAdresiYok = "Denetim kaydı servisi adresi tanımlı değil.";

    public const string CrudDenetimAciklamasi = "{0} {1} işlemi başarıyla tamamlandı.";
}
