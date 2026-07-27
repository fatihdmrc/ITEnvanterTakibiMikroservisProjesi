using System.ComponentModel.DataAnnotations;

namespace EnvanterTakip.MvcClient.Models;

public sealed class KimlikPersonelPanelModel
{
    public OturumKullaniciModel? OturumKullanici { get; set; }
    public IReadOnlyCollection<DepartmanModel> Departmanlar { get; set; } = [];
    public IReadOnlyCollection<PersonelModel> Personeller { get; set; } = [];
    public IReadOnlyCollection<KullaniciModel> Kullanicilar { get; set; } = [];
    public string? BasariMesaji { get; set; }
    public string? HataMesaji { get; set; }
}

public sealed class GirisFormModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string KullaniciAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;
}

public sealed class DepartmanOlusturFormModel
{
    [Required(ErrorMessage = "Departman adı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    public Guid? SorumluPersonelId { get; set; }
}

public sealed class PersonelOlusturFormModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girilmelidir.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departman zorunludur.")]
    public Guid DepartmanId { get; set; }

    [Required(ErrorMessage = "Unvan zorunludur.")]
    public string Unvan { get; set; } = string.Empty;

    public bool DepartmanSorumlusuMu { get; set; }
    public DateOnly IseGirisTarihi { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public sealed class KullaniciOlusturFormModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string KullaniciAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;

    public KullaniciRoluModel Rol { get; set; } = KullaniciRoluModel.PersonelKullanicisi;

    [Required(ErrorMessage = "Personel zorunludur.")]
    public Guid PersonelId { get; set; }
}

public sealed record GirisCevapModel(
    string Token,
    Guid KullaniciId,
    Guid PersonelId,
    KullaniciRoluModel Rol,
    DateTimeOffset GecerlilikZamani);

public sealed record OturumKullaniciModel(
    string? KullaniciId,
    string? KullaniciAdi,
    string? PersonelId,
    string? Rol);

public sealed record DepartmanModel(
    Guid Id,
    string Ad,
    Guid? SorumluPersonelId,
    bool AktifMi);

public sealed record PersonelModel(
    Guid Id,
    string Ad,
    string Soyad,
    string Email,
    Guid DepartmanId,
    string Unvan,
    bool DepartmanSorumlusuMu,
    PersonelDurumuModel Durum,
    DateOnly IseGirisTarihi,
    DateOnly? IstenAyrilisTarihi,
    bool AktifMi);

public sealed record KullaniciModel(
    Guid Id,
    string KullaniciAdi,
    KullaniciRoluModel Rol,
    Guid PersonelId,
    bool AktifMi);

public enum KullaniciRoluModel
{
    Admin = 1,
    ITPersoneli = 2,
    PersonelKullanicisi = 3
}

public enum PersonelDurumuModel
{
    Aktif = 1,
    Pasif = 2,
    IstenAyrildi = 3
}
